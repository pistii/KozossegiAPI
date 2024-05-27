using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KozoskodoAPI.Repo
{
    public class PersonalRepository : GenericRepository<Personal>, IPersonalRepository
    {
        private readonly DBContext _context;

        public PersonalRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Első körbe adja vissza lakóhely szerint az adatokat. This currently works alphabetically descending but later will be added by geolocation in a specific radius
        // In the next round give back the data by years, added +10 year to the user and from that value it descends.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IQueryable<Personal> FilterPersons(int userId)
        {
            var query = _context.Personal
                .OrderByDescending(_ => _.PlaceOfResidence)
                .OrderByDescending(_ => _.DateOfBirth.Value.AddYears(10))
                .Where(_ => _.id != userId);
            return query;
        }
    }
}
