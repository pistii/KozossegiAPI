using KozossegiAPI.Auth;
using KozossegiAPI.Data;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.SMTP;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Resources;

namespace KozossegiAPI.Repo
{
    public class UserRepository : GenericRepository<user>, IUserRepository<user>
    {
        private readonly DBContext _dbContext;
        private readonly IJwtTokenManager _jwtTokenManager;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMailSender _mailSender;
        protected readonly string URL_BASE = "https://192.168.0.16:8888";

        public UserRepository(DBContext context, 
            IJwtTokenManager jwtTokenManager,
            IJwtUtils jwtUtils,
            IMailSender mailSender) : base(context)
        {
            _dbContext = context;
            _jwtTokenManager = jwtTokenManager;
            _jwtUtils = jwtUtils;
            _mailSender = mailSender;
        }


        public async Task<Personal?> GetPersonalWithSettingsAndUserAsync(int userId)
        {
            var user = await _dbContext.Personal
                .Include(p => p.Settings)
                .Include(u => u.users)
                .Include(s => s.activeStudy)
                .FirstOrDefaultAsync(p => p.users.userID == userId);
            return user;
        }

        public async Task<Personal?> GetPersonalWithSettingsAndUserAsync(string userId)
        {
            var user = await _dbContext.Personal
                .Include(p => p.Settings)
                .Include(u => u.users)
                .Include(s => s.activeStudy)
                .FirstOrDefaultAsync(p => p.users.PublicId == userId);
            return user;
        }

        /// <summary>
        /// Sends a request by email and/or password;
        /// </summary>
        /// <param name="email"></param>
        /// <returns>The user found by the parameters</returns>
        public async Task<user?> GetUserByEmailOrPassword(string email = null, string password = null)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.email == email && u.password == password);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.email == email);
            }
            else if (!string.IsNullOrEmpty(password))
            {
                return await _dbContext.user.FirstOrDefaultAsync(u => u.password == password);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds the user with the email. Personal table included.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="withPersonal"></param>
        /// <returns></returns>
        public async Task<user?> GetUserByEmailAsync(string email, bool withPersonal = true)
        {
            if (withPersonal)
            {
                return await _dbContext.user.Include(p => p.personal).FirstOrDefaultAsync(user => user.email == email);
            }
            else
            {
                var query =  await _dbContext.user.FirstOrDefaultAsync(user => user.email == email);
                query.personal = null;
                return query;
            }
        }

        public async Task SendActivationEmail(string email, user user)
        {
            var token = _jwtUtils.GenerateAccessToken(email, 1440);

            ResourceManager resourceManager = new ResourceManager("KozossegiAPI.translation", typeof(Translations).Assembly);
            var currentCulture = CultureInfo.CurrentCulture;

            string htmlTemplate = _mailSender.getEmailTemplate("register.html");
            var url = $"{URL_BASE}/validate/{token}";

            string? one_step = resourceManager.GetString("one_step", currentCulture);
            string? click_link = resourceManager.GetString("click_link", currentCulture);
            string? subject = resourceManager.GetString("confirm_registration", currentCulture);
            string? inform = resourceManager.GetString("email_because", currentCulture);

            htmlTemplate = htmlTemplate.Replace("{ONE_STEP}", one_step);
            htmlTemplate = htmlTemplate.Replace("{CLICK_LINK}", click_link);
            htmlTemplate = htmlTemplate.Replace("{URL}", url.ToString());
            htmlTemplate = htmlTemplate.Replace("{URL_LINK}", url);
            htmlTemplate = htmlTemplate.Replace("{EMAIL_BECAUSE}", inform.ToString());

            _mailSender.SendEmail(subject!, htmlTemplate, user.personal.firstName + " " + user.personal.lastName, email);
        }

        public async Task<bool> CanUserRequestMoreActivatorToday(string email)
        {
            user? user = await _dbContext.user
                .Include(p => p.personal)
                .FirstOrDefaultAsync(user => user.email == email);

            var userRestrictionIds = _dbContext.UserRestriction
                .Where(p => p.UserId == user.userID)
                .Select(p => p.RestrictionId)
                .ToList();

            var restrictionsToday = _dbContext.Restriction.Where(
                p => userRestrictionIds.Contains(p.RestrictionId) &&
                p.HappenedOnDate.Date == DateTime.Today && 
                p.Description == "User requests email activator email request")
                .Count();

            if (restrictionsToday > 4)
            {
                return false;
            }

            Restriction restriction = new()
            {
                Description = "User requests email activator email request",
                FK_StatusId = 2
            };
            await _dbContext.Restriction.AddAsync(restriction);
            await _dbContext.SaveChangesAsync();

            UserRestriction userRestriction = new()
            {
                UserId = user.userID,
                RestrictionId = restriction.RestrictionId
            };
            await _dbContext.UserRestriction.AddAsync(userRestriction);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
