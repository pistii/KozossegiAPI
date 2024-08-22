using KozoskodoAPI.Data;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozoskodoAPI.Repo;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class ImageRepository : IImageRepository
    {
        private IStorageRepository _storageRepository;
        private DBContext _ctx;

        public ImageRepository(IStorageRepository repository, DBContext dbContext)
        {
            _storageRepository = repository;
            _ctx = dbContext;
        }

        public async Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9)
        {
            var postsWithImage = await _ctx.PersonalPost
                .Include(p => p.Posts.MediaContent)
                .Include(p => p.Posts.PostComments)
                .Where(p => p.Posts.SourceId == userId && p.Posts.MediaContent != null) //Ennyiben tér el a sima post kérelemtől, hogy még ezt kell vizsgálni
                .OrderByDescending(_ => _.Posts.DateOfPost)
                .Take(requestItems)
                .Select(p => new PostDto
                {
                    PersonalPostId = p.personalPostId,
                    FullName = $"{p.Personal_posts.firstName} {p.Personal_posts.middleName} {p.Personal_posts.lastName}",
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
                            CommenterFirstName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).firstName!,
                            CommenterMiddleName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).middleName!,
                            CommenterLastName = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).lastName!,
                            CommenterAvatar = _ctx.Personal.First(_ => _.id == c.FK_AuthorId).avatar!,
                            CommentDate = c.CommentDate,
                            CommentText = c.CommentText!
                        })
                        .ToList(),
                    MediaContent = p.Posts.MediaContent
                })
                .ToListAsync();
            return postsWithImage;

        }

        public async Task<MediaContent> GetPostImage(int postId)
        {
            return await _ctx.MediaContent.FirstOrDefaultAsync(c => c.MediaContentId == postId);
        }

        public async Task UpdateDatabaseImageUrl(int userId, string url)
        {
            var user = await _ctx.Personal.FirstOrDefaultAsync(u => u.id == userId);
            user.avatar = url;
            await _ctx.SaveChangesAsync();
        }

    }
}
