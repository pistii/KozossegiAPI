using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo {
    public class NavigationRepository : INavigationRepository
    {
        private readonly DBContext _dBContext;
        
        public NavigationRepository(DBContext dbContext)
        {
            _dBContext = dbContext;
        }

        public async Task<List<RecommendedPerson>> SearchForPerson(string searchByValue, int page, int itemPerRequest)
        {
            //return null;
            var queryParameter = searchByValue.ToLower();


            var result = await _dBContext.Personal
                .Include(f => f.friends)
                .Include(p => p.users)
                .Where(x =>
                    x.lastName.ToLower().Contains(queryParameter) ||
                    x.firstName.ToLower().Contains(queryParameter) ||
                    (x.middleName != null && x.middleName.ToLower().Contains(queryParameter)) ||
                    x.PlaceOfBirth != null && x.PlaceOfBirth.ToLower().Contains(queryParameter) ||
                    x.PlaceOfResidence != null && x.PlaceOfResidence.ToLower().Contains(queryParameter))
                .Select(p => new RecommendedPerson(p) {
                    Score = 0,
                    PublicId = p.users!.PublicId
                })
                .Skip((page - 1) * itemPerRequest)
                .Take(itemPerRequest)
                .ToListAsync();

            return result;
        }

        public async Task<List<RecommendedPosts>> SearchForPost(int currentUser, string searchByValue, int page, int itemPerRequest)
        {
            var queryParam = searchByValue.ToLower();

            var result = await _dBContext.PersonalPost
                .Include(p => p.Author)
                    .ThenInclude(u => u.users)
                .Include(p => p.Receiver)
                .Include(p => p.Posts)
                    .ThenInclude(p => p.MediaContent)
                .Include(r => r.Posts)
                    .ThenInclude(p => p.PostReactions)
                .Where(x =>
                    x.Posts.PostContent != null && x.Posts.PostContent.ToLower().Contains(queryParam))
                .Select(p => new RecommendedPosts
                {
                    PostAuthor = new PostAuthor(
                        p.Author.avatar ?? "",
                        p.Author.firstName,
                        p.Author.middleName ?? "",
                        p.Author.lastName,
                        p.Author.users!.PublicId),
                    Post = new Post(p.PostId, p.Posts.Token, p.Posts.PostContent ?? "", 
                    p.Posts.PostReactions != null ?
                    p.Posts.PostReactions.Count(c => c.ReactionTypeId == 1) : 0, //like
                    p.Posts.PostReactions != null ? 
                    p.Posts.PostReactions.Count(c => c.ReactionTypeId == 2) : 0, //dislike
                    p.Posts.DateOfPost, p.Posts.LastModified, p.Posts.MediaContent),
                    PostedToUserId = p.Author.users.PublicId,
                    IsAuthor = p.AuthorId == currentUser,
                    CommentsQty = p.Posts.PostComments.Count,
                    Score = 0
                })
                .Skip((page - 1) * itemPerRequest)
                .Take(itemPerRequest)
                .ToListAsync();
            
            return result;
        }
    }


}