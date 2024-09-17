using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Repo;
using KozossegiAPI.SMTP;
using Microsoft.AspNetCore.Mvc;
using Bcrypt = BCrypt.Net.BCrypt;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : ControllerBase
    {
        public DBContext _context;
        private ISettingRepository _settingRepository;
        private IStudyRepository _studyRepository;
        private IMailSender _mailSender;
        public SettingController(DBContext dbContext, 
            ISettingRepository settingRepository, 
            IStudyRepository studyRepository,
            IMailSender mailSender
            )
        {
            _context = dbContext;
            _settingRepository = settingRepository;
            _studyRepository = studyRepository;
            _mailSender = mailSender;
        }

        [Authorize]
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserData(int id)
        {
            object? user = await _settingRepository.GetSettings(id);
            if (user != null)
            {
                return Ok(user);
            }
            return BadRequest();
        }

        /// <summary>
        /// The method used when the user completes the registration process, or modifies the personal information about him/herself.
        /// </summary>
        /// <param name="userInfoDTO"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [Authorize]
        public async Task<IActionResult?> Update(SettingDto userInfoDTO)
        {
            var userContext = (user?)HttpContext.Items["User"];
            Personal? user = await _settingRepository.GetPersonalWithSettingsAndUserAsync(userInfoDTO.User.userID);

            if (user == null || userContext == null) return BadRequest();

            var userStudies = userInfoDTO.User.StudiesDto.ToList();
            if (userStudies != null)
            {
                userStudies.ForEach(s => s.FK_UserId = userContext.userID);
            }

            var result = await _studyRepository.UpdateStudies(userStudies);
            if (result != System.Net.HttpStatusCode.OK)
                return BadRequest();

            //A public study id beállítása
            if (user.users.Studies.Any(p => p.PK_Id == userInfoDTO.User.selectedStudyId))
            {
                user.publicStudyId = (int)userInfoDTO.User.selectedStudyId;
            } 
            else
            {
                var study = user.users.Studies.FirstOrDefault(p => p.initId == userInfoDTO.User.selectedStudyId);
                if (study != null) 
                    user.publicStudyId = study.PK_Id;
            }

            if (userInfoDTO.SecuritySettings != null &&
                (userInfoDTO.SecuritySettings.pass1 == userInfoDTO.SecuritySettings.pass2) &&
                userInfoDTO.SecuritySettings.pass1.Length > 8)
            {
                user.users.password = Bcrypt.HashPassword(userInfoDTO.SecuritySettings.pass1);
            }

            await _settingRepository.UpdateChanges(user, userInfoDTO);

            #region //Emai l sending

            string fullName = user.middleName == null ? user.firstName + " " + user.lastName : user.firstName + " " + user.middleName + " " + user.lastName;

            var htmlTemplate = _mailSender.getEmailTemplate("userDataChanged.html");
            htmlTemplate.Replace("{userFullName}", user.lastName);

            //_mailSender.SendEmail("Módosítás történt a felhasználói fiókodban", htmlTemplate, fullName, user.users.email);
            #endregion

            return Ok();
        }

    }
}
