using Google.Apis.Storage.v1.Data;
using KozoskodoAPI.Auth.Helpers;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Repo;
using KozossegiAPI.Services;
using KozossegiAPI.SMTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Linq;
using System.Threading.Channels;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingController : ControllerBase
    {
        public DBContext _context;
        private ISettingRepository _settingRepository;
        private ISettingService _settingService;
        private IStudyRepository _studyRepository;
        
        public SettingController(DBContext dbContext, 
            ISettingRepository settingRepository, 
            ISettingService settingService,
            IStudyRepository studyRepository)
        {
            _context = dbContext;
            _settingRepository = settingRepository;
            _settingService = settingService;
            _studyRepository = studyRepository;
        }

        //[Authorize]
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

        //[Authorize]
        [HttpDelete("remove/study/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            user? user = (user?)HttpContext.Items["User"];
            var userExist = await _context.user.FirstOrDefaultAsync(user => user.userID == id);

            if (userExist != null)
            {
                //Search the study from the user's all studies, then remove it
                var studyToRemove = await _context.Study.FirstOrDefaultAsync(p => p.PK_Id == userExist.userID && p.FK_UserId == id);
                if (studyToRemove != null)
                {
                    await _settingRepository.RemoveThenSaveAsync(studyToRemove);
                    return Ok();
                }
                return NotFound();
            }
            return BadRequest();
        }


        /// <summary>
        /// The method used when the user completes the registration process, or modifies the personal information about him/herself.
        /// </summary>
        /// <param name="userInfoDTO"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [AllowAnonymous]
        public async Task<IActionResult> Update([FromForm] ModifyUserInfoDTO userInfoDTO)
        {
            Personal? user = await _settingRepository.GetPersonalWithSettingsAndUserAsync(userInfoDTO.UserId);

            if (user == null) return BadRequest();
            
            //visszaadja a kész Personal táblát a módosult gyermek táblákkal együtt, illetve azt hogy történt-e módosítás az emailt vagy jelszót illetőleg.
            var modifications = _settingService.ModifyUserDataIfChanged(userInfoDTO, user);
            var emailOrPasswordChanged = modifications.Value;

            try
            {
                await _settingRepository.UpdateAvatarIfChanged(userInfoDTO);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error on avatar upload. (P1:file, P2:userId)", userInfoDTO.File, userInfoDTO.UserId);
                return null;
            }

            var userStudies = userInfoDTO.Studies;
            if (userStudies != null)
            {
                if (userStudies.Count() > 0)
                {
                    await _studyRepository.UpdateStudies(modifications.Key.users.Studies.ToList());
                }
            }
            await _settingRepository.UpdateThenSaveAsync(modifications.Key);
            //_settingService.SendEmailIfEmailOrPasswordChanged(emailOrPasswordChanged, modifications.Key);

            return Ok(userInfoDTO); //nem kell visszaadni az adatokat
        }

    }
}
