using Microsoft.AspNetCore.Mvc;
using KozoskodoAPI.Models;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Data;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.DTOs;
using System.Security.Claims;
using KozoskodoAPI.Repo;
using KozoskodoAPI.SMTP;
using Humanizer;
using Google.Api;
using System.Web;
using File = System.IO.File;
using KozoskodoAPI.SMTP.Storage;
using BCrypt.Net;
using Microsoft.AspNetCore.Routing.Template;
using System.ComponentModel.DataAnnotations;
using KozoskodoAPI.Security;
using Microsoft.AspNetCore.DataProtection;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.Sockets;
using System.Drawing.Imaging;

namespace KozoskodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class usersController : ControllerBase
    {
        private readonly IJwtTokenManager _jwtTokenManager;
        private readonly IJwtUtils _jwtUtils;
        private readonly DBContext _context;
        private readonly IFriendRepository _friendRepository;
        private readonly IPostRepository _postRepository;
        private readonly IMailSender _mailSender;
        private readonly IVerificationCodeCache _verCodeCache;
        private readonly IEncodeDecode _encodeDecode;
        protected readonly string URL_BASE = "http://localhost:5173/";
        
        public usersController(
            IJwtTokenManager jwtTokenManager, 
            IJwtUtils jwtUtils, 
            IFriendRepository friendRepository, 
            IPostRepository postRepository, 
            IMailSender mailSender,
            IVerificationCodeCache verCodeCache,
            IEncodeDecode encodeDecode,
            DBContext context)
        {
            _jwtTokenManager = jwtTokenManager;
            _jwtUtils = jwtUtils;
            _context = context;
            _friendRepository = friendRepository;
            _postRepository = postRepository;
            _mailSender = mailSender;
            _verCodeCache = verCodeCache;
            _encodeDecode = encodeDecode;
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto login)
        {
            var response = _jwtTokenManager.Authenticate(login);
            if (response == null)
            {
                return NotFound("Username or password is incorrect");
            }

            //var userData = _context.user.Include(x => x.personal).First(x => x.email == login.Email && 
            //BCrypt.Net.BCrypt.Verify(login.Password, x.password));
            //var personalInfo = await _context.Personal.FindAsync(userData.userID); //Modified from personalID

            var userId = response.personal.id;
            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, login.Email),
                new Claim(ClaimTypes.GivenName, "A user"),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            AuthenticateResponse userDto = new AuthenticateResponse(response.personal!, response.token, identity.Claims);
            return Ok(userDto);
        }
        
        // GET: user
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _context.Personal.FindAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpGet("profilePage/{profileToViewId}/{viewerUserId}")]
        public async Task<IActionResult> GetProfilePage(int profileToViewId, int viewerUserId)
        {

            var user = await _context.Personal.FindAsync(profileToViewId);
            if (user != null)
            {
                try
                {
                    var posts = await _postRepository.GetAllPost(profileToViewId, viewerUserId);
                    var friends = await _friendRepository.GetAll(profileToViewId);
                    var familiarityStatus = await _friendRepository.GetFamiliarityStatus(profileToViewId, viewerUserId);
                    ProfilePageDto profilePageDto = new ProfilePageDto()
                    {
                        PersonalInfo = user,
                        Posts = posts,
                        Friends = friends,
                        PublicityStatus = familiarityStatus
                    };
                    return Ok(profilePageDto);

                }
                catch (Exception ex)
                {
                    return NotFound("Something went wrong...");
                }
            }
            return NotFound();

        }

        [HttpPost("Signup")]
        [AllowAnonymous]
        public async Task<ActionResult<user>> SignUp(RegisterForm user)
        {

            if (user != null)
            {
                //user? userExistsByEmail = await _context.user.FirstOrDefaultAsync(u => u.email == user.email);
                //if (userExistsByEmail != null)
                //{
                //    return Ok("used email");
                //}

                user newUser = user;
                newUser.password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                //Guid: https://stackoverflow.com/a/4458925/16689442
                Guid guid = Guid.NewGuid();
                user.Guid = guid.ToString("N");
                _context.user.Add(newUser);
                await _context.SaveChangesAsync();

                var token = _jwtUtils.GenerateJwtToken(newUser); //TODO: generate 15min access tokens

                string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates"); //TODO: this will fail in production
                string content = Path.Combine(fullpath, "register.html");

                string htmlTemplate = System.IO.File.ReadAllText(content);
                string url = $"{URL_BASE}validate/{token}";
                htmlTemplate = htmlTemplate.Replace("{Url}", url);

                //_mailSender.SendMail("Sikeres regisztráció", htmlTemplate, newUser.personal.firstName + " " + newUser.personal.lastName, newUser.email);

                return Ok("success");
            }
            return BadRequest("error");
        }

        /// <summary>
        /// This method is used when the user registered and the required to activate the email
        /// </summary>
        /// <returns></returns>
        [HttpGet("Validate")]
        public async Task<IActionResult> ValidateToken()
        {

            var user = (user?)HttpContext.Items["User"]; //Get the user from headers


            if (user != null)
            {
                user? userExists = await _context.user.FirstOrDefaultAsync(u => u.email == user.email && u.password == user.password);

                if (userExists != null && !userExists.isActivated)
                {
                    userExists.isActivated = true;

                    await _context.SaveChangesAsync();
                    return Ok(userExists); //Sikeres aktiválás

                }
                return NotFound(); // Már aktivált felhasználó
            }
            return BadRequest(); //Hibás a token, vagy már lejárt
        }

        /// <summary>
        /// This endpoint activates when the user forgots the password, and sends the OTP to the email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ForgotPw")]
        public async Task<IActionResult> ForgotPw([FromBody]  EncryptedDataDto dto) 
        {
            var decryptedEmail = _encodeDecode.Decrypt(dto.Data, "I love chocolate");
            int verificationCode = 123456;
            user? user = await _context.user.Include(p => p.personal).FirstOrDefaultAsync(user => user.email == decryptedEmail);
            if (user != null)
            {
                //var token = _jwtUtils.GenerateJwtToken(user); //TODO: generate 15min access tokens
                //string link = $"{URL_BASE}" + HttpUtility.UrlPathEncode($"reset-pw/{token}");
                Random random = new Random();
                verificationCode = random.Next(100000, 999999);

                _verCodeCache.Create(verificationCode.ToString(), user.Guid);

                string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
                string templatePath = Path.Combine(fullpath, "forgotPassword.html");

                string htmlTemplate = System.IO.File.ReadAllText(templatePath);
                htmlTemplate = htmlTemplate.Replace("{LastName}", user.personal.lastName);
                htmlTemplate = htmlTemplate.Replace("{VerificationCode}", verificationCode.ToString());

                //TODO: Send email
                //_mailSender.SendMail("Elfelejtett jelszó", htmlTemplate, user.personal.firstName + " " + user.personal.lastName, user.email!);

            }
            return Ok(_encodeDecode.Encrypt(verificationCode.ToString(), "I love chocolate"));

        }


        [AllowAnonymous]
        [HttpPost("checkVerCode")]
        public async Task<IActionResult> IsVercodeCorrect(EncryptedDataDto dto) 
        {
            string verCode = _encodeDecode.Decrypt(dto.Data, "I love chocolateI love chocolate");
            var matchVercode = _verCodeCache.GetValue(verCode);
            if (!string.IsNullOrEmpty(matchVercode))
            {
                var user = await _context.user.FirstOrDefaultAsync(u => u.Guid == matchVercode);
                if (user != null)
                {
                    _verCodeCache.Remove(verCode);

                    user.password = BCrypt.Net.BCrypt.HashPassword(verCode);
                    _context.user.Update(user);
                    await _context.SaveChangesAsync();

                    return Ok(dto);
                }
            }
            return NotFound("");
        }



        [HttpPost("Restrict")]
        [AllowAnonymous] //TODO: IT'S IMPORTANT
        public async Task<IActionResult> RestrictUser(RestrictionDto data)
        {
            user? user = await _context.user.FindAsync(data.userId);
            if (user != null)
            {
                Restriction restriction = new Restriction()
                {
                    Description = data.Description,
                    EndDate = data.EndDate,
                    FK_StatusId = data.FK_StatusId
                };

                _context.Restriction.Add(restriction);
                await _context.SaveChangesAsync();

                UserRestriction userRestriction = new UserRestriction()
                {
                    RestrictionId = restriction.RestrictionId,
                    UserId = user.userID
                };

                _context.UserRestriction.Add(userRestriction);
                await _context.SaveChangesAsync();



                return Ok();
            }
            return NotFound();
        } 

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, user user)
        {
            if (id != user.userID)
            {
                return BadRequest();
            }
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(userExists(id)))
                    return NotFound();
                return BadRequest();
            }
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _context.user.FindAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            _context.user.Remove(result);

            _context.SaveChangesAsync().Wait();
            return Ok();
        }

        public bool userExists(int id)
        {
            return _context.user.Any(e => e.userID == id);
        }

    }

}