using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.SMTP;
using Microsoft.AspNetCore.Mvc;
using Bcrypt = BCrypt.Net.BCrypt;

namespace KozossegiAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : BaseController<SettingController>
    {
        private readonly ISettingRepository _settingRepository;
        private readonly IStudyRepository _studyRepository;
        private readonly IMailSender _mailSender;

        public SettingController(
            ISettingRepository settingRepository, 
            IStudyRepository studyRepository,
            IMailSender mailSender
            )
        {
            _settingRepository = settingRepository;
            _studyRepository = studyRepository;
            _mailSender = mailSender;
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetUserData(string id)
        {
            var user = await _settingRepository.GetByPublicIdAsync<user>(id);
            var userSettings = await _settingRepository.GetSettings(user.userID);
            if (userSettings != null)
            {
                return Ok(userSettings);
            }
            return BadRequest();
        }

        /// <summary>
        /// The method used when the user completes the registration process, or modifies the personal information about him/herself.
        /// </summary>
        /// <param name="userInfoDTO"></param>
        /// <returns></returns>
        [HttpPut("update")]
        public async Task<IActionResult?> Update(SettingDto userInfoDTO)
        {
            var userContext = GetUser();
            Personal? user = await _settingRepository.GetPersonalWithSettingsAndUserAsync(userContext.userID);

            if (user == null || userContext == null) return BadRequest();

            var userStudies = userInfoDTO.User.StudiesDto.ToList();
            if (userStudies != null)
            {
                userStudies.ForEach(s => s.FK_UserId = userContext.userID);
            }

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
                (userInfoDTO.SecuritySettings.pass1 == userInfoDTO.SecuritySettings.pass2))
            {
                user.users.password = Bcrypt.HashPassword(userInfoDTO.SecuritySettings.pass1);
            }
            else
            {
                return BadRequest("Password doesn't match");
            }


            var result = await _studyRepository.UpdateStudies(userStudies);
            if (result != System.Net.HttpStatusCode.OK)
                return BadRequest();

            await _settingRepository.UpdateChanges(user, userInfoDTO);

            //_mailSender.UserDataChangedEmail(user);
            return Ok();
        }

    }
}
