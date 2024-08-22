using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozossegiAPI.DTOs;
using System.Net;

namespace KozossegiAPI.Repo
{
    public interface IStudyRepository : IGenericRepository<Study>
    {
        Task<HttpStatusCode> UpdateStudies(List<StudyDto> changes);
    }
}
