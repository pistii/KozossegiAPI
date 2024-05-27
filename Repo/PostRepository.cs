using Humanizer;
using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozossegiAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KozoskodoAPI.Repo
{
    public class PostRepository : GenericRepository<PostDto>, IPostRepository<PostDto>, IPostRepository<Comment>
    {
        private readonly DBContext _context;
        private HelperService helperService;
        public PostRepository(DBContext context) : base(context)
        {
            _context = context;
            helperService = new();
        }

        /// <summary>
        /// Get all post with comments
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="userId"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemPerRequest"></param>
        /// <returns>PostDto as post with the comments and the media content.</returns>
        public async Task<List<PostDto>> GetAllPost(int profileId, int userId)
        {
            var sortedItems = await _context.PersonalPost
                .Include(p => p.Posts.MediaContents)
                .Include(p => p.Posts.PostComments)
                .Where(p => p.Posts.SourceId == profileId)
                .OrderByDescending(_ => _.Posts.DateOfPost)
                .Select(p => new PostDto
                {
                    PersonalPostId = p.personalPostId,
                    FullName = helperService.GetFullname(p.Personal_posts.firstName!, p.Personal_posts.middleName, p.Personal_posts.lastName!), //p.Personal_posts.firstName! + " " + p.Personal_posts.middleName + " " + p.Personal_posts.lastName!, //TODO
                    PostId = p.Posts.Id,
                    AuthorAvatar = p.Personal_posts.avatar!,
                    AuthorId = p.Personal_posts.id,
                    Likes = p.Posts.Likes,
                    Dislikes = p.Posts.Dislikes,
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
                    MediaContents = p.Posts.MediaContents.ToList(),
                    userReaction = _context.PostReaction.FirstOrDefault(u => p.Posts.Id == u.PostId && u.UserId == userId).ReactionType
                })
                .ToListAsync();
            return sortedItems;
        }


        public async Task<Post?> GetPostWithCommentsById(int postId)
        {
            var post = await _context.Post
                    .Include(p => p.PostComments)
                    .FirstOrDefaultAsync(p => p.Id == postId);
            return post;
        }
    }
}
