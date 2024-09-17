using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace KozossegiAPI.Repo
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudyRepository : GenericRepository<Study>, IStudyRepository
    {
        private readonly DBContext _dbContext;
        public StudyRepository(DBContext context) : base(context)
        {
            _dbContext = context;
        }


        public async Task<HttpStatusCode> UpdateStudies(List<StudyDto> changes)
        {
            List<StudyDto> studiesToBeModified = changes.Where(s => s.Status == "modified").ToList();
            if (studiesToBeModified.Count == 1)
            {
                var modifyTo = studiesToBeModified.First();
                var study = await GetByIdAsync<Study>((int)modifyTo.Id);

                if (study == null)
                {
                    return HttpStatusCode.NotFound;
                }
                study.Class = modifyTo.Class;
                study.SchoolName = modifyTo.SchoolName;
                study.StartYear = modifyTo.StartYear;
                study.EndYear = modifyTo.EndYear;

                await UpdateAsync(study);
            }
            else if (studiesToBeModified.Count > 1)
            {
                var studyIds = studiesToBeModified.Select(s => s.Id).ToList();
                var studies = _dbContext.Study.Where(s => studyIds.Contains(s.PK_Id));

                foreach (var study in studies)
                {
                    var rs = studiesToBeModified.First(p => p.Id == study.PK_Id);
                    study.SchoolName = rs.SchoolName;
                    study.StartYear = rs.StartYear;
                    study.EndYear = rs.EndYear;
                    study.Class = rs.Class;
                }

                _dbContext.Study.UpdateRange(studies);
            }

            List<Study> newStudies = changes.Where(s => s.Status == "new")
                .Select(study => new Study()
            {
                StartYear = study.StartYear,
                EndYear = study.EndYear,
                Class = study.Class,
                SchoolName = study.SchoolName,
                FK_UserId = study.FK_UserId,
                initId = study.initId
            }).ToList();
            if (newStudies.Count > 0)
            {
                await _dbContext.AddRangeAsync(newStudies);
            }

            await SaveAsync();
            return HttpStatusCode.OK;
        }
    }
}
