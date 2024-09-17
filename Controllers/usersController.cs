using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Bcry = BCrypt.Net.BCrypt;
using System.Text.RegularExpressions;
using KozossegiAPI.SMTP;
using KozossegiAPI.Storage;
using KozossegiAPI.Services;
using KozossegiAPI.Security;
using KozossegiAPI.Auth;
using KozossegiAPI.Repo;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Auth.Helpers;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class usersController : ControllerBase
    {
        private readonly IJwtTokenManager _jwtTokenManager;
        private readonly IJwtUtils _jwtUtils;

        private readonly IFriendRepository _friendRepository;

        private readonly IImageRepository _imageRepository;
        private readonly IPostRepository<PostDto> _postRepository;
        private readonly IUserRepository<user> _userRepository;

        private readonly IMailSender _mailSender;
        private readonly IVerificationCodeCache _verCodeCache;
        private readonly IEncodeDecode _encodeDecode;
        protected readonly string URL_BASE = "http://localhost:5173/";
        
        public usersController(
            IJwtTokenManager jwtTokenManager,
            IJwtUtils jwtUtils,
            IFriendRepository friendRepository,
            IPostRepository<PostDto> postRepository,
            IImageRepository imageRepository,
            IUserRepository<user> userRepository,

            IMailSender mailSender,
            IVerificationCodeCache verCodeCache,
            IEncodeDecode encodeDecode
            )
        {
            _jwtTokenManager = jwtTokenManager;
            _jwtUtils = jwtUtils;
            _friendRepository = friendRepository;
            _postRepository = postRepository;
            _imageRepository = imageRepository;
            _userRepository = userRepository;

            _mailSender = mailSender;
            _verCodeCache = verCodeCache;
            _encodeDecode = encodeDecode;
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto login)
        {
            var response = await _jwtTokenManager.Authenticate(login);
            if (response == null)
            {
                return NotFound("Username or password is incorrect");
            }

            var userId = response?.personal?.id;

            var identity = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, login.Email),
                new Claim(ClaimTypes.GivenName, "A user"),
                new Claim(ClaimTypes.NameIdentifier, userId?.ToString())
            });

            AuthenticateResponse userDto = new AuthenticateResponse(response.personal!, response.token);
            return Ok(userDto);
        }

        // GET: user
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var user = await _userRepository.GetuserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpGet("profilePage/{profileToViewId}/{viewerUserId}")]
        [Authorize]
        public async Task<IActionResult> GetProfilePage(int profileToViewId, int viewerUserId)
        {
            var user = await _userRepository.GetPersonalWithSettingsAndUserAsync(profileToViewId);
            const int REMINDER_OF_UNFULFILLED_PERSONAL_INFOS_IN_DAYS = 7;
            if (user != null)
            {
                try
                {
                    var posts = await _postRepository.GetAllPost(profileToViewId, viewerUserId, 1);
                    var friends = await _friendRepository.GetAll(profileToViewId);
                    var familiarityStatus = await _friendRepository.CheckIfUsersInRelation(profileToViewId, viewerUserId);

                    //await Task.WhenAll(postsTask, familiarityStatusTask);

                    //var posts = await postsTask;
                    //var friends = friends;
                    //var familiarityStatus = await familiarityStatusTask;

                    bool reminduser = false;

                    if (familiarityStatus == "self" && user.users.LastOnline.Year == 1 || //If first login
                        familiarityStatus == "self" && (DateTime.UtcNow - user.users.LastOnline).TotalDays > REMINDER_OF_UNFULFILLED_PERSONAL_INFOS_IN_DAYS //if the last login was more than 7 days ago,
                        )  
                    {
                        if (user.Settings == null)
                            reminduser = true;
                        else if (familiarityStatus == "self" && DateTime.UtcNow > user.Settings.NextReminder)
                        {
                            reminduser = true;
                        }
                        else reminduser = false;
                    }

                    ProfilePageDto profilePageDto = new ProfilePageDto()
                    {
                        PersonalInfo = user,
                        Posts = posts,
                        Friends = friends.ToList(),
                        PublicityStatus = familiarityStatus,
                        settings = new()
                        {
                            isOnlineEnabled = user.users.isOnlineEnabled,
                            RemindUserOfUnfulfilledReg = reminduser
                        }
                        
                        
                    };
                    return Ok(profilePageDto);

                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("Something went wrong.... " + ex);
                }
            }
            return NotFound();
        }

        //TODO: The method itself is not so relevant and if there will be implemented an interval communication between the server, this method also can be used cabcatenated
        /// <summary>
        /// Turns off or extends the reminder of unfulfilled registration process.
        /// </summary>
        /// <returns></returns>
        [HttpPut("turnOffReminder")]
        public async Task<IActionResult> TurnOffReminder(UserSettingsDTO dto)
        {
            var user = (user?)HttpContext.Items["User"];
            var personal = await _userRepository.GetPersonalWithSettingsAndUserAsync(user.userID);

            var userSettings = personal.Settings;
            if (userSettings == null)
            {
                userSettings = new Settings();
                userSettings.FK_UserId = user.userID;
                userSettings.NextReminder = DateTime.Now.AddDays(1);
            }
            userSettings.NextReminder.AddDays(dto.Days);

            await _userRepository.InsertSaveAsync<Settings>(userSettings);
            return Ok("Next reminder: " + userSettings.NextReminder);
        }

        [HttpPost("Signup")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp(RegisterForm user)
        {

            if (user != null && user.email != null)
            {
                user? userExistsByEmail = await _userRepository.GetUserByEmailAsync(user.email);
                if (userExistsByEmail == null)
                {
                    return BadRequest("used email");
                }

                user newUser = user;
                newUser.password = Bcry.HashPassword(user.Password);
                //Guid: https://stackoverflow.com/a/4458925/16689442
                Guid guid = Guid.NewGuid();
                user.Guid = guid.ToString("N");

                await _userRepository.InsertSaveAsync<user>(newUser);

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
        public async Task<IActionResult> ActivateUser()
        {
            var user = (user?)HttpContext.Items["User"]; //Get the user from headers

            if (user != null)
            {
                user? userExists = await _userRepository.GetUserByEmailOrPassword(user.email, user.password);
                if (userExists != null && !userExists.isActivated)
                {
                    userExists.isActivated = true;
                    await _userRepository.UpdateThenSaveAsync(userExists);
                    return Ok(userExists); //Sikeres aktiválás
                }
                return NotFound(); // Már aktivált felhasználó
            }
            return BadRequest(); //Hibás a token, vagy már lejárt
        }


        [HttpGet("password/check/{token}")]
        public async Task<IActionResult> CheckIfEmailValid(string token)
        {
            return Ok(_jwtUtils.ValidateAccessToken(token));
        }
        /// <summary>
        /// This is the process to request a change a new password when user requests a new one 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("password/reset")]
        public async Task<IActionResult> PasswordReset_1([FromBody] EncryptedDataDto dto) 
        {
            //TODO: Add rate limiting to requests: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
            //var ip = HttpContext.Connection.RemoteIpAddress.ToString();

            var decryptedData = dto.Data; //_encodeDecode.EncryptAesManaged(dto.Data);

            int min=100000, max=999999;
                        
            if (Regex.IsMatch(decryptedData, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$")) { //Email validation

                user? user = await _userRepository.GetUserByEmailAsync(decryptedData);
                if (user != null)
                {
                    string access_token = _jwtUtils.GenerateAccessToken(dto.Data);
                    string verificationCode = OTPKey.GenerateKey();
                    _verCodeCache.Create(verificationCode.ToString(), user.Guid);

                    //string htmlTemplate = getEmailTemplate("forgotPassword.html");
                    //htmlTemplate = htmlTemplate.Replace("{LastName}", user.personal.lastName);
                    //htmlTemplate = htmlTemplate.Replace("{VerificationCode}", verificationCode);

                    ////TODO: Send email
                    //try
                    //{
                    //    _mailSender.SendMail("Elfelejtett jelszó", htmlTemplate, user.personal.firstName + " " + user.personal.lastName, user.email!);
                    //} catch (Exception ex)
                    //{
                    //    Log.Error(ex, "Email sending error");
                    //}

                    HttpContext.Request.Headers["Authentication"] = access_token;
                    return Ok(access_token);
                }
                return NotFound();
            }

            else if (int.TryParse(decryptedData, out int num) && num >= min && num <= max)
            {
                string token = HttpContext.Request.Headers["Authentication"];

                string? email = _jwtUtils.ValidateAccessToken(token);
                if (email != null)
                {
                    var userGuid = _verCodeCache.GetValue(decryptedData);
                    if (!string.IsNullOrEmpty(userGuid))
                    {
                        return Ok();
                    }
                }
                return Unauthorized();
            }

            return BadRequest();
        }

        //[HttpPost("password/reset/otp")]
        //public async Task<IActionResult> PasswordReset_2([FromForm] )

        [HttpPut("password/modify")]
        [Authenticate]
        public async Task<IActionResult> ModifyPassword(ModifyPassword form)
        {
            if (form.Password1 == form.Password2 && form.otpKey != null)
            {
                if (!HelperService.PasswordIsValid(form.Password1)) { 
                    return BadRequest("not a valid password.");
                }
                var userguid = _verCodeCache.GetValue(form.otpKey);
                var user = await _userRepository.GetByGuid(userguid);

                if (user != null)
                {
                    user.password = Bcry.HashPassword(form.Password1);

                    _verCodeCache.Remove(form.otpKey);
                    await _userRepository.UpdateThenSaveAsync(user);
                    return Ok();
                }
                return NotFound();
            }
            return BadRequest();
        }

        [HttpPost("Restrict")]
        [Authorize]
        public async Task<IActionResult> RestrictUser(RestrictionDto data)
        {
            user? user = await _userRepository.GetuserByIdAsync(data.userId);
            if (user != null)
            {
                Restriction restriction = new Restriction()
                {
                    Description = data.Description,
                    EndDate = data.EndDate,
                    FK_StatusId = data.FK_StatusId
                };

                await _userRepository.InsertSaveAsync<Restriction>(restriction);
                
                UserRestriction userRestriction = new UserRestriction()
                {
                    RestrictionId = restriction.RestrictionId,
                    UserId = user.userID
                };

                await _userRepository.InsertSaveAsync<UserRestriction>(userRestriction);
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
            try
            {
                await _userRepository.UpdateThenSaveAsync(user);
            }
            catch (Exception e)
            {
                var exists = await _userRepository.GetByIdAsync<user>(id);
                if (exists == null)
                    return NotFound();
                return BadRequest();
            }
            return NoContent();
        }

        //Used from connectionHandler, this method seems like is not needed 
        //public async Task LogoutUser()
        //{
        //    var user = (user?)HttpContext.Items["User"];
        //    var userById = await _userRepository.GetByIdAsync<user>(user.userID);
        //    userById.LastOnline = DateTime.Now;
        //    return;
        //}


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userRepository.GetByIdAsync<user>(id);
            if (result == null)
            {
                return NotFound();
            }
            await _userRepository.RemoveThenSaveAsync<user>(result);
            return Ok();
        }


    }
}