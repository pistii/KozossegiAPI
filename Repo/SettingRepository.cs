using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class SettingRepository : GenericRepository<Study>, ISettingRepository
    {
        private readonly DBContext _dbContext;

        public SettingRepository(
            DBContext context
            ) : base(context)
        {
            _dbContext = context;
        }

        /// <summary>
        /// Returns the user with settings, more personal info and the studies table
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<object?> GetSettings(int userId)
        {
            var user = await _dbContext.Personal
                .Include(p => p.Settings)
                .Include(u => u.users)
                .ThenInclude(s => s.Studies)
                .FirstOrDefaultAsync(p => p.id == userId);

            var studies = user.users.Studies.Select(s => new StudyDto()
            {
                Id = s.PK_Id,
                FK_UserId = s.FK_UserId,
                Class = s.Class,
                StartYear = s.StartYear,
                EndYear = s.EndYear,
                SchoolName = s.SchoolName,
                Status = "original"
            }).AsEnumerable();

            var userDto = new UserDto(user.users, studies);
            var settings = new SettingDto(userDto, null);

            return settings;
        }

        /// <summary>
        /// Queryes the Personal table with it's dependent childs table: Setting, users, Studies
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId)
        {
            var user = await _dbContext.Personal
                .Include(p => p.Settings)
                .Include(u => u.users)
                .ThenInclude(p => p.Studies)
                .FirstOrDefaultAsync(p => p.id == userId);
            return user;
        }



        public async Task UpdateChanges(Personal user, SettingDto userInfoDTO)
        {
            user.firstName = userInfoDTO.User.personal.firstName;
            user.lastName = userInfoDTO.User.personal.lastName;
            user.middleName = userInfoDTO.User.personal.middleName;
            user.PlaceOfResidence = userInfoDTO.User.personal.PlaceOfResidence;
            user.PlaceOfBirth = userInfoDTO.User.personal.PlaceOfBirth;
            user.Workplace = userInfoDTO.User.personal.Workplace;
            user.Profession = userInfoDTO.User.personal.Profession;
            user.DateOfBirth = userInfoDTO.User.personal.DateOfBirth;
            user.Profession = userInfoDTO.User.personal.Profession;
            user.phoneNumber = userInfoDTO.User.personal.phoneNumber;
            user.users.isOnlineEnabled = userInfoDTO.User.isOnlineEnabled;
            user.users.email = userInfoDTO.User.email;

            if (user.users.email != userInfoDTO.User.SecondaryEmailAddress)
                user.users.SecondaryEmailAddress = userInfoDTO.User.SecondaryEmailAddress;
            
            await  UpdateThenSaveAsync(user);
        }

    }
}
