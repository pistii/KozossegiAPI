using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NavigationController : ControllerBase
    {
        private readonly DBContext _context;
        private readonly IFriendRepository _friendRepository;

        public NavigationController(DBContext context, IFriendRepository friendRepository)
        {
            _context = context;
            _friendRepository = friendRepository;
        }

        [HttpGet("search/{userId}/{search}")]
        public async Task<List<ProfilePageDto>> Search(
            int userId,
            int page = 1,
            int itemPerRequest = 25,
            string? search = null)
        {
            //search for people
            var result = _context.Personal
                .Where(x =>
                x.middleName.ToLower().Contains(search.ToLower()) ||
                x.lastName.ToLower().Contains(search.ToLower()) ||
                x.firstName.ToLower().Contains(search.ToLower()))
                .OrderByDescending(x => x.PlaceOfResidence)
                .OrderByDescending(x => x.PlaceOfBirth)
                .OrderByDescending(x => x.DateOfBirth)
                .AsQueryable();

            result = result.Skip((page - 1) * itemPerRequest).Take(itemPerRequest);
            var toSend = await result.ToListAsync();

            var allFriend = await _friendRepository.GetAllFriendAsync(userId);

            var relation = await result.Select(person => new ProfilePageDto()
            {
                PersonalInfo = person,
                PublicityStatus = allFriend.Contains(person) ? "friend" : "non-friend"
            }).ToListAsync();

            return relation;

            //TODO: post tartalmak kigyűjtése amennyiben tartalmazza a keresési kulcsszót. a szűrés Vegye figyelembe hogy csak az ismerősök és a saját tartalmak között keressen, továbbá keverje a keresési eredményeket attól függően hogy mennyi eredményt talált a post és a személyek szűrésekor.
            /*
             * //search for people
            var persons = _context.Personal
                .Where(x =>
                x.middleName.ToLower().Contains(search.ToLower()) ||
                x.lastName.ToLower().Contains(search.ToLower()) ||
                x.firstName.ToLower().Contains(search.ToLower()))
                .OrderByDescending(x => x.PlaceOfResidence)
                .OrderByDescending(x => x.PlaceOfBirth)
                .OrderByDescending(x => x.DateOfBirth)
                .AsQueryable();

            var personResult = await persons.Skip((th - 1) * itemPerRequest).Take(itemPerRequest).ToListAsync();

            await persons.ToListAsync();
            //Search in posts
            var posts = await _context.PersonalPost
                .Include(p => p.Posts.MediaContents)
                .Include(p => p.Posts.PostComments)
                .Where(p => p.Posts.SourceId == userId && p.Posts.PostContent.Contains(search))
                .OrderByDescending(_ => _.Posts.DateOfPost)
                .Skip((itemPerRequest - 1) * itemPerRequest)
                .Take(itemPerRequest)
                .Select(p => new PostDto
                {
                    PersonalPostId = p.personalPostId,
                    FullName = $"{p.Personal_posts.firstName} {p.Personal_posts.middleName} {p.Personal_posts.lastName}",
                    PostId = p.Posts.Id,
                    AuthorAvatar = p.Personal_posts.avatar!,
                    AuthorId = p.Personal_posts.id,
                    Likes = p.Posts.Loves,
                    Dislikes = p.Posts.DisLoves,
                    DateOfPost = p.Posts.DateOfPost,
                    PostContent = p.Posts.PostContent!,
                    PostComments = p.Posts.PostComments
                        .Select(c => new CommentDto
                        {
                            CommentId = c.commentId,
                            AuthorId = c.FK_AuthorId,
                            CommenterFirstName = _context.Personal.First(_ => _.id == c.FK_AuthorId).firstName!,
                            CommenterMiddleName = _context.Personal.First(_ => _.id == c.FK_AuthorId).middleName!,
                            CommenterLastName = _context.Personal.First(_ => _.id == c.FK_AuthorId).lastName!,
                            CommenterAvatar = _context.Personal.First(_ => _.id == c.FK_AuthorId).avatar!,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText!
                        })
                        .ToList(),
                    MediaContents = p.Posts.MediaContents.ToList()
                })
                .ToListAsync();

            // Kinyerünk két egyenlő méretű csoportot a személyekből és a posztokból
            var shuffledPersons = persons.OrderBy(x => Guid.NewGuid()).Take(itemPerRequest / 2);
            var shuffledPosts = posts.OrderBy(x => Guid.NewGuid()).Take(itemPerRequest / 2);

            // Az összekevert csoportokat egyesével összefésüljük
            var result = await shuffledPersons.Zip(shuffledPosts, (person, post) => new SearchDto
            {
                Person = person,
                Post = post
            
            })
            .ToListAsync();

            */
        }
    }
}
