using KozossegiAPI.Auth.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KozossegiAPI.Interfaces;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudyController : ControllerBase
    {
        public DBContext _context;
        private IStudyRepository _studyRepository;

        public StudyController(DBContext context, 
            IStudyRepository studyRepository)
        {

            _context = context;
            _studyRepository = studyRepository;
        }


        [Authorize]
        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            user? user = (user?)HttpContext.Items["User"];
            var userExist = await _context.Personal
                .Include(u => u.users)
                .ThenInclude(s => s.Studies)
                .FirstOrDefaultAsync(u => u.id == user.userID);

            if (userExist != null)
            {
                //Search the study from the user's all studies, then remove it
                var studyToRemove = userExist.users.Studies.FirstOrDefault(s => s.PK_Id == id);
                if (studyToRemove != null)
                {
                    await _studyRepository.RemoveThenSaveAsync(studyToRemove);
                    return Ok();
                }
                else if (userExist.publicStudyId == id)
                {
                    userExist.publicStudyId = 0;
                }
                return NotFound();
            }
            return BadRequest();
        }

    }
}
