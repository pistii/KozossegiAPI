using Microsoft.AspNetCore.Mvc;
using Bcry = BCrypt.Net.BCrypt;
using System.Text.RegularExpressions;
using KozossegiAPI.SMTP;
using KozossegiAPI.Storage;
using KozossegiAPI.Services;
using KozossegiAPI.Security;
using KozossegiAPI.Auth;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Auth.Helpers;
using ILogger = Serilog;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Repo.Helper;
using Serilog;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : BaseController<UserController>
    {
        private readonly IJwtTokenManager _jwtTokenManager;
        private readonly IJwtUtils _jwtUtils;

        private readonly IFriendRepository _friendRepository;

        private readonly IImageRepository _imageRepository;
        private readonly IPostRepository<PostDto> _postRepository;
        private readonly IUserRepository<user> _userRepository;
        private readonly ISettingRepository _settingRepository;

        private readonly IPermissionHelper _permissionHelper;

        private readonly IMailSender _mailSender;
        private readonly IVerificationCodeCache _verCodeCache;
        private readonly IEncodeDecode _encodeDecode;
        protected readonly string URL_BASE = "https://192.168.0.16:8888";
        
        public UserController(
            IJwtTokenManager jwtTokenManager,
            IJwtUtils jwtUtils,

            IFriendRepository friendRepository,
            IPostRepository<PostDto> postRepository,
            IImageRepository imageRepository,
            IUserRepository<user> userRepository,
            ISettingRepository settingRepository,

            IPermissionHelper permissionHelper,

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
            _settingRepository = settingRepository;

            _permissionHelper = permissionHelper;

            _mailSender = mailSender;
            _verCodeCache = verCodeCache;
            _encodeDecode = encodeDecode;
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(LoginDto login)
        {
            var response = await _jwtTokenManager.Authenticate(login);
            if (response == null) return NotFound("Username or password is incorrect");
            if (!response.UserDetails.IsActivated) return BadRequest("Not activated");

            return Ok(response);
        }

        [Authorize]
        [HttpGet("profile/{otherUserId}")]
        public async Task<IActionResult> GetPermissions(string otherUserId)
        {
            int userId = GetUserId();
            var userWithPersonal = await _userRepository.GetWithIncludeAsync<Personal, user>(u => u.users, u => u.users.PublicId == otherUserId);

            if (userWithPersonal == null) return NotFound("User not found");

            var permissions = await _permissionHelper.ResolveAsync(userWithPersonal, userId, userWithPersonal.id);
            return Ok(permissions);
        }

        //[Authorize]
        //[HttpGet("get/userData")]
        //public async Task<IActionResult> GetInitUserData()
        //{
        //    int userId = GetUserId();

        //    //UserDetailsDto userDetailsDto = new();

        //    return Ok();
        //}

        [Authorize]
        [HttpGet("myProfile")]
        public async Task<IActionResult> GetMyProfile()
        {
            int userId = GetUserId();
            var user = await _userRepository.GetByIdAsync<Personal>(userId);
            if (user == null) return NotFound("User not found");

            try
            {
                var postsTask = _postRepository.GetAllPost(user.id, userId, 1);
                var friendsTask = _friendRepository.GetAll(user.id);

                await Task.WhenAll(postsTask, friendsTask);

                var posts = await postsTask;
                var friends = await friendsTask;


                ProfilePageDto profilePageDto = new()
                {
                    PersonalInfo = user,
                    Posts = posts,
                    Friends = friends.ToList(),
                    Identity = UserRelationshipStatus.Self,
                    settings = new()
                    {
                        //isOnlineEnabled = user.users.isOnlineEnabled,
                        //RemindUserOfUnfulfilledReg = reminduser,
                        //PostEnabled = userCanPost
                    },
                    PublicId = user.users.PublicId
                        
                };
                return Ok(profilePageDto);

            }
            catch (Exception ex)
            {
                
                Console.WriteLine("Something went wrong.... " + ex);
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
            if (user.personal == null) return BadRequest("Invalid form");

            user? userExistsByEmail = await _userRepository.GetUserByEmailAsync(user.email);
            if (userExistsByEmail != null) return BadRequest("used email");

            user newUser = user;
            newUser.password = Bcry.HashPassword(user.Password);
            //Guid: https://stackoverflow.com/a/4458925/16689442
            Guid guid = Guid.NewGuid();
            user.PublicId = guid.ToString("N");

            await _userRepository.InsertSaveAsync<user>(newUser);
            //TODO:URGENT
            //await _userRepository.SendActivationEmail(user.email, newUser);
            return Ok();   
        }

        [AllowAnonymous]
        [HttpPost("register/send/email-activator/{to}")]
        public async Task<IActionResult> SendEmailActivator(string to)
        {
            user? userExistsByEmail = await _userRepository.GetUserByEmailAsync(to);
            if (userExistsByEmail == null) return NotFound();

            var requestGranted = await _userRepository.CanUserRequestMoreActivatorToday(to);
            if (!userExistsByEmail.isActivated && requestGranted)
            {
                await _userRepository.SendActivationEmail(to, userExistsByEmail);
            }
            return Ok();
        }

        /// <summary>
        /// This method is used when the user registered and the required to activate the email
        /// </summary>
        /// <returns></returns>
        [HttpGet("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ActivateUser()
        {
            var token = HttpContext.Request.Headers["Validation-token"];
            var email = _jwtUtils.ValidateAccessToken(token);

            if (email != null)
            {
                user? userExists = await _userRepository.GetUserByEmailAsync(email);
                if (userExists != null && !userExists.isActivated)
                {
                    userExists.isActivated = true;
                    await _userRepository.UpdateThenSaveAsync(userExists);
                    return Ok(); //Sikeres aktiválás
                }
                return NotFound(); // Már aktivált felhasználó
            }
            return BadRequest(); //Hibás a token, vagy már lejárt
        }


        /// <summary>
        /// This is the process to request a change a new password when user requests a new one 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("password/reset/email/")]
        //[EnableRateLimiting("password_reset")]
        public async Task<IActionResult> PasswordReset_1(EncryptedDataDto dto) 
        {
            var ip = HttpContext.Connection?.RemoteIpAddress?.ToString();
            if (ip != null)
            {
                Log.Information("Password reset request from ip: " + ip + ". For email: " + dto.Data);
            }

            var decryptedData = dto.Data;
                        
            if (Regex.IsMatch(decryptedData, "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$")) { //email validáció

                user? user = await _userRepository.GetUserByEmailAsync(decryptedData);
                string verificationCode = OTPKey.GenerateKey();

                if (user != null)
                {
                    _verCodeCache.Create(verificationCode.ToString(), user.PublicId);
                    //CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

                    string htmlTemplate = _mailSender.getEmailTemplate("forgotPassword.html");
                    htmlTemplate = htmlTemplate.Replace("{LastName}", user.personal.lastName ?? "Felhasználó");
                    htmlTemplate = htmlTemplate.Replace("{VerificationCode}", verificationCode);

                    try
                    {
                        _mailSender.SendEmail("Elfelejtett jelszó", 
                            htmlTemplate, user.personal.firstName + " " + user.personal.lastName, user.email!);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Email sending error", "email: " + user.email);
                    }

                }

                string access_token = _jwtUtils.GenerateAccessToken(dto.Data+";"+verificationCode, 5);
                    HttpContext.Request.Headers["Authentication"] = access_token;
                    return Ok(access_token);
                }
            return ValidationProblem();
            }

        [HttpPost("password/reset/otp")]
        [Authenticate]
        public async Task<IActionResult> PasswordReset_2(OneTimePassword dto)
        {
            int min = 100000, max = 999999;
            
            if (int.TryParse(dto.otpKey, out int num) && num >= min && num <= max)
            {
                string? header = HttpContext.Request.Headers["Authentication"];
                if (header == null) 
                    return BadRequest(); //Lehet hogy már lejárt és már nem érvényes vagy hibás.

                string token = header.Split(" ").Last();
                string? email = _jwtUtils.ValidateAccessToken(token);
                if (email != null)
                {
                    var userGuid = _verCodeCache.GetValue(dto.otpKey);
                    if (!string.IsNullOrEmpty(userGuid))
                    {
                        return Ok();
                    }
                }
                return Unauthorized();
            }

            return BadRequest();
        }

        [HttpPost("password/reset/modify")]
        [Authenticate]
        public async Task<IActionResult> ModifyPassword(ModifyPassword form)
        {
            if (form.Password1 == form.Password2 && form.otpKey != null)
            {
                if (!HelperService.PasswordIsValid(form.Password1)) { 
                    return BadRequest("not a valid password.");
                }
                var userguid = _verCodeCache.GetValue(form.otpKey);
                var user = await _userRepository.GetByPublicIdAsync<user>(userguid);

                if (user != null)
                {
                    user.password = Bcry.HashPassword(form.Password1);

                    _verCodeCache.Remove(form.otpKey);
                    await _userRepository.UpdateThenSaveAsync(user);
                    //TODO: értesítés küldése a módosított jelszó miatt
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
            user? user = await _userRepository.GetByIdAsync<user>(data.userId);
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
        [AllowAnonymous]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _userRepository.GetByIdAsync<Personal>(id);
            var user = await _userRepository.GetByIdAsync<user>(id);

            if (result == null)
            {
                return NotFound();
            }
            await _userRepository.RemoveThenSaveAsync<Personal>(result);
            await _userRepository.RemoveThenSaveAsync<user>(user);

            return Ok();
        }


    }
}