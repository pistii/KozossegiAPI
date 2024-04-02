using KozoskodoAPI.Models;
using KozoskodoAPI.Auth;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Repo;
using KozoskodoAPI.SMTP;
using KozoskodoAPI.SMTP.Storage;
using KozoskodoAPI.Security;
using KozoskodoAPI.DTOs;

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using KozoskodoAPI.Controllers.Cloud;
using Google.Api;
using Newtonsoft.Json;
using Bcry = BCrypt.Net.BCrypt;
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
            IEncodeDecode encodeDecode,
            DBContext context
            )
        {
            _jwtTokenManager = jwtTokenManager;
            _jwtUtils = jwtUtils;
            _context = context;
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

            var userData = _context.user.Include(x => x.personal).First(x => x.email == login.Email);
            if (!BCrypt.Net.BCrypt.Verify(login.Password, userData.password))
            {
                return BadRequest();
            }
            //var personalInfo = await _context.Personal.FindAsync(userData.userID); //Modified from personalID

            var userId = userData.personal.id;
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
            //var user = await _context.Personal.FindAsync(id);
            var user = await _userRepository.GetuserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }
            return NotFound();
        }

        [HttpGet("profilePage/{profileToViewId}/{viewerUserId}")]
        public async Task<IActionResult> GetProfilePage(int profileToViewId, int viewerUserId)
        {

            //var user = await _context.Personal.Include(p => p.Settings).Include(u => u.users).FirstOrDefaultAsync(p => p.id == profileToViewId);
            var user = _userRepository.GetPersonalWithSettingsAndUserAsync(profileToViewId).Result;
            const int REMINDER_OF_UNFULFILLED_PERSONAL_INFOS_IN_DAYS = 7;
            if (user != null)
            {
                try
                {
                    PostController postController = new(_postRepository);
                    var posts = postController.GetAllPost(profileToViewId, viewerUserId, 1).Result;

                    var friends = _friendRepository.GetAll(profileToViewId).Result;
                    var familiarityStatus = _friendRepository.CheckIfUsersInRelation(profileToViewId, viewerUserId).Result;
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
            //var personal = await _context.Personal.Include(s => s.Settings).FirstOrDefaultAsync(p => p.id == user.userID);
            var personal = await _userRepository.GetPersonalWithSettingsAndUserAsync(user.userID);

            var userSettings = personal.Settings;
            if (userSettings == null)
            {
                userSettings = new Settings();
                userSettings.FK_UserId = user.userID;
                userSettings.NextReminder = DateTime.Now.AddDays(1);
            }
            
            userSettings.NextReminder.AddDays(dto.Days);

            //_context.Settings.Update(userSettings);
            //await _context.SaveChangesAsync();
            await _userRepository.InsertSaveAsync<Settings>(userSettings);

            return Ok("Next reminder: " + userSettings.NextReminder);
        }

        [HttpPost("Signup")]
        [AllowAnonymous]
        public async Task<ActionResult<user>> SignUp(RegisterForm user)
        {

            if (user != null && user.email != null)
            {
                //user? userExistsByEmail = await _context.user.FirstOrDefaultAsync(u => u.email == user.email);
                user? userExistsByEmail = await _userRepository.GetUserByEmailOrPassword(user.email);
                if (userExistsByEmail != null)
                {
                    return Ok("used email");
                }

                user newUser = user;
                newUser.password = Bcry.HashPassword(user.Password);
                //Guid: https://stackoverflow.com/a/4458925/16689442
                Guid guid = Guid.NewGuid();
                user.Guid = guid.ToString("N");
                //_context.user.Add(newUser);
                //await _context.SaveChangesAsync();
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
        /// The method used when the user completes the registration process, or modifies the personal information about him/herself.
        /// </summary>
        /// <param name="userInfoDTO"></param>
        /// <returns></returns>
        [HttpPut("modify")]
        [AllowAnonymous]
        public async Task<IActionResult> ModifyUserInfo([FromForm] ModifyUserInfoDTO userInfoDTO)
        {

            //user? user = await _context.user.Include(p => p.personal).Include(s => s.Studies).FirstOrDefaultAsync(p => p.userID == userInfoDTO.UserId);
            Personal? user = await _userRepository.GetPersonalWithSettingsAndUserAsync(userInfoDTO.UserId);
            bool emailOrPasswordChanged = false;

            if (user != null)
            {//will be checked the values individually because the user can update one, or more info and don't want to change the non null values to null.
                if (!string.IsNullOrEmpty(userInfoDTO.Name) && userInfoDTO.File != null)
                {
                    AvatarUpload avatarUpload = new AvatarUpload(userInfoDTO.UserId, userInfoDTO.Name, userInfoDTO.Type, userInfoDTO.File);

                    await _imageRepository.Upload(avatarUpload);
                }
                if (!string.IsNullOrEmpty(userInfoDTO.firstName))
                {
                    user.firstName = userInfoDTO.firstName;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.middleName))
                {
                    user.middleName = userInfoDTO.middleName;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.lastName))
                {
                    user.lastName = userInfoDTO.lastName;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.PlaceOfResidence))
                {
                    user.PlaceOfResidence = userInfoDTO.PlaceOfResidence;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.PhoneNumber))
                {
                    user.phoneNumber = userInfoDTO.PhoneNumber;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.PlaceOfBirth))
                {
                    user.PlaceOfBirth = userInfoDTO.PlaceOfBirth;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.EmailAddress))
                {
                    emailOrPasswordChanged = true;
                    user.users.email = userInfoDTO.EmailAddress;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.SecondaryEmailAddress))
                {
                    emailOrPasswordChanged = true;
                    if (userInfoDTO.SecondaryEmailAddress != user.users.email) //Egyéb hibakezelésre nincs szükség, mivel a frontend elvégzi
                    {
                        user.users.SecondaryEmailAddress = userInfoDTO.SecondaryEmailAddress;
                    }
                }
                if (!string.IsNullOrEmpty(userInfoDTO.Profession))
                {
                    user.Profession = userInfoDTO.Profession;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.Workplace))
                {
                    user.Workplace = userInfoDTO.Workplace;
                }
                if (!string.IsNullOrEmpty(userInfoDTO.Pass1) && !string.IsNullOrEmpty(userInfoDTO.Pass2) && userInfoDTO.Pass1 == userInfoDTO.Pass2)
                {
                    emailOrPasswordChanged = true;
                    user.users.password = Bcry.HashPassword(userInfoDTO.Pass1);
                }
                user.users.isOnlineEnabled = userInfoDTO.isOnline;

                if (emailOrPasswordChanged)
                {
                    //send an email about the email/password change
                    string fullName;
                    if (!string.IsNullOrEmpty(user.middleName))
                    {
                        fullName = user.firstName + " " + user.middleName + " " + user.lastName;
                    }
                    else
                    {
                        fullName = user.firstName + " " + user.lastName;
                    }

                    var htmlTemplate = getEmailTemplate("userDataChanged.html");
                    htmlTemplate.Replace("{userFullName}", user.lastName);
                    //TODO: Enable the email sending
                    //_mailSender.SendMail("Módosítás történt a felhasználói fiókodban", htmlTemplate, fullName, user.email);
                }

                if ((!string.IsNullOrEmpty(userInfoDTO.SchoolName) || !string.IsNullOrEmpty(userInfoDTO.Class) || userInfoDTO.StartYear != null || userInfoDTO.EndYear != null) )
                {
                    
                    if (user.users.Studies == null)
                    {
                        Studies newStudy = new Studies(user.id, userInfoDTO.SchoolName, userInfoDTO.Class, DateOnly.Parse(userInfoDTO.StartYear ?? null), DateOnly.Parse(userInfoDTO.EndYear ?? null));
                        await _context.Studies.AddAsync(newStudy);
                    } else
                    {
                        var userStudy = await _context.Studies.FirstOrDefaultAsync(s => s.FK_UserId == user.id);
                        if (!string.IsNullOrEmpty(userInfoDTO.SchoolName))
                        {
                            userStudy.SchoolName = userInfoDTO.SchoolName;
                        }
                        if (!string.IsNullOrEmpty(userInfoDTO.Class))
                        {
                            userStudy.Class = userInfoDTO.Class;
                        }
                        if (userInfoDTO.StartYear != null)
                        {
                            userStudy.StartYear = DateOnly.Parse(userInfoDTO.StartYear ?? null);
                        }
                        if (userInfoDTO.EndYear != null)
                        {
                            userStudy.EndYear = DateOnly.Parse(userInfoDTO.EndYear ?? null);
                        }
                    }
                }

                //_context.user.Update(user.users);
                //await _context.SaveChangesAsync();
                _userRepository.UpdateThenSaveAsync(user);

                return Ok(user.users);
            }
            return NotFound();
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
                //user? userExists = await _context.user.FirstOrDefaultAsync(u => u.email == user.email && u.password == user.password);
                user? userExists = await _userRepository.GetUserByEmailOrPassword(user.email, user.password);
                if (userExists != null && !userExists.isActivated)
                {
                    userExists.isActivated = true;

                    //await _context.SaveChangesAsync();
                    await _userRepository.SaveAsync();
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
            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            var decryptedEmail = _encodeDecode.Decrypt(dto.Data, "I love chocolate");
            int verificationCode = 123456;
            //user? user = await _context.user.Include(p => p.personal).FirstOrDefaultAsync(user => user.email == decryptedEmail);
            user? user = await _userRepository.GetUserByEmailAsync(decryptedEmail);
            if (user != null)
            {
                //var token = _jwtUtils.GenerateJwtToken(user); //TODO: generate 15min access tokens
                //string link = $"{URL_BASE}" + HttpUtility.UrlPathEncode($"reset-pw/{token}");
                Random random = new Random();
                verificationCode = random.Next(100000, 999999);

                _verCodeCache.Create(verificationCode.ToString(), user.Guid);

                string htmlTemplate = getEmailTemplate("forgotPassword.html");

                htmlTemplate = htmlTemplate.Replace("{LastName}", user.personal.lastName);
                htmlTemplate = htmlTemplate.Replace("{VerificationCode}", verificationCode.ToString());

                //TODO: Send email
                //_mailSender.SendMail("Elfelejtett jelszó", htmlTemplate, user.personal.firstName + " " + user.personal.lastName, user.email!);

            }
            return Ok(_encodeDecode.Encrypt(verificationCode.ToString(), "I love chocolate"));

        }


        [AllowAnonymous]
        [HttpPost("checkVerCode")]
        public async Task<IActionResult> IsVercodeCorrect(EncryptedDataDto dto)  //TODO: Add rate limiting to requests: https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0
        {
            string verCode = _encodeDecode.Decrypt(dto.Data, "I love chocolateI love chocolate");
            //var splitted = data.Split('&');
            //var verCode = splitted[0];
            //var tries = splitted[1];
            var matchVercode = _verCodeCache.GetValue(verCode);
            if (!string.IsNullOrEmpty(matchVercode))
            {
                //var user = await _context.user.FirstOrDefaultAsync(u => u.Guid == matchVercode);
                var user = await _userRepository.GetByGuid(matchVercode);
                if (user != null)
                {
                    user.password = Bcry.HashPassword(verCode);
                    //_context.user.Update(user);
                    //await _context.SaveChangesAsync();
                    await _userRepository.InsertSaveAsync(user);
                    return Ok(dto);
                }
            }
            return NotFound("");
        }

        [HttpPost("password/modify")]
        [AllowAnonymous]
        public async Task<IActionResult> ModifyPassword(ModifyPassword form)
        {
            if (form.Password1 == form.Password2 && form.otpKey != null)
            {
                var userguid = _verCodeCache.GetValue(form.otpKey);

                var user = await _userRepository.GetByGuid(userguid);
                if (user != null)
                {
                    user.password = Bcry.HashPassword(form.Password1);
                    //_context.user.Update(user);
                    //await _context.SaveChangesAsync();

                    _verCodeCache.Remove(form.otpKey);
                    await _userRepository.UpdateThenSaveAsync(user);
                    return Ok();
                }
                return NotFound();
            }
            return BadRequest();
        }

        [HttpPost("Restrict")]
        [AllowAnonymous] //TODO: IT'S IMPORTANT
        public async Task<IActionResult> RestrictUser(RestrictionDto data)
        {
            //user? user = await _context.user.FindAsync(data.userId);
            user? user = await _userRepository.GetuserByIdAsync(data.userId);
            if (user != null)
            {
                Restriction restriction = new Restriction()
                {
                    Description = data.Description,
                    EndDate = data.EndDate,
                    FK_StatusId = data.FK_StatusId
                };

                //_context.Restriction.Add(restriction);
                //await _context.SaveChangesAsync();
                await _userRepository.InsertSaveAsync<Restriction>(restriction);
                
                UserRestriction userRestriction = new UserRestriction()
                {
                    RestrictionId = restriction.RestrictionId,
                    UserId = user.userID
                };

                //_context.UserRestriction.Add(userRestriction);
                //await _context.SaveChangesAsync();
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
            //_context.Entry(user).State = EntityState.Modified;
            try
            {
                await _userRepository.UpdateThenSaveAsync(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _userRepository.GetByIdAsync<user>(id);
                if (exists == null)
                    return NotFound();
                return BadRequest();
            }
            return NoContent();
        }

        public async Task LogoutUser()
        {
            var user = (user?)HttpContext.Items["User"];
            //var user = _context.user.FindAsync(userId.userID).Result;
            var userById = await _userRepository.GetByIdAsync<user>(user.userID);
            userById.LastOnline = DateTime.Now;
            return;
        }

        /// <summary>
        /// Waits a fileName as parameter which will be used as a template.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>
        /// The template with the given name.
        /// </returns>
        public string getEmailTemplate(string fileName)
        {
            string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
            string templatePath = Path.Combine(fullpath, fileName);
            return System.IO.File.ReadAllText(templatePath);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            //var result = await _context.user.FindAsync(id);
            var result = await _userRepository.GetByIdAsync<user>(id);
            if (result == null)
            {
                return NotFound();
            }
            //_context.user.Remove(result);
            //_context.SaveChangesAsync().Wait();
            await _userRepository.RemoveThenSaveAsync<user>(result);
            return Ok();
        }

    }
}