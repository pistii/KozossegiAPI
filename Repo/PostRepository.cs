using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Repo.Helper;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class PostRepository : GenericRepository<PostDto>, IPostRepository
    {
        private readonly DBContext _context;
        private readonly IStorageRepository _storageRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IPermissionHelper _permissionHelper;

        public PostRepository(
            DBContext context,
            IStorageRepository storageRepository,
            IFriendRepository friendRepository,
            IPermissionHelper permissionHelper
            ) : base( context )
        {
            _context = context;
            _storageRepository = storageRepository;
            _friendRepository = friendRepository;
            _permissionHelper = permissionHelper;
        }

        public async Task<bool> UserCanCreatePost(int postAuthorId, int postedToUserId)
        {
            var postedToUserWithSetting = await GetWithIncludeAsync<Settings, Personal>(i => i.personal, p => p.FK_UserId == postedToUserId);
            var status = await _friendRepository.GetRelationStatusAsync(postAuthorId, postedToUserWithSetting.FK_UserId);
            return _permissionHelper.CanPostAccordingToSettings(status, postedToUserWithSetting.PostCreateEnabledToId);
        }

        /// <summary>
        /// Get all post with comments
        /// </summary>_pos
        /// <param name="profileId"></param>
        /// <param name="userId"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemPerRequest"></param>
        /// <returns>PostDto as post with the comments and the media content.</returns>
        public async Task<ContentDto<PostDto>> GetAllPost(int profileId, string publicUserId, int currentPage = 1, int itemPerRequest = 10)
        {
            var sortedItems = await _context.PersonalPost
            .Include(p => p.Posts.MediaContent)
            .Include(c => c.Posts.PostComments)
            .Include(p => p.Posts.PostReactions)
            .Where(p => p.PostedToId == profileId)
            .OrderByDescending(_ => _.Posts.DateOfPost)
            .AsNoTracking()
            .Select(p => new PostDto
            {
                PostAuthor = new PostAuthor(
                    p.Author.avatar,
                    p.Author.firstName,
                    p.Author.middleName,
                    p.Author.lastName,
                    p.Author.users.PublicId),
                Post = new Post(p.PostId, p.Posts.Token, p.Posts.PostContent, 
                p.Posts.PostReactions.Count(c => c.ReactionTypeId == 1), //like
                 p.Posts.PostReactions.Count(c => c.ReactionTypeId == 2), //dislike
                p.Posts.DateOfPost, p.Posts.LastModified, p.Posts.MediaContent),
                PostedToUserId = publicUserId,
                IsAuthor = p.Author.users.PublicId == publicUserId,
                CommentsQty = p.Posts.PostComments.Count,
            })
            .ToListAsync();

            if (sortedItems == null) return null;
            int totalPages = await GetTotalPages(sortedItems, itemPerRequest);
            var returnValue = Paginator(sortedItems, currentPage, itemPerRequest).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        public async Task<List<PostDto>> GetImagesAsync(int userId, string publicId)
        {
            var sortedItems = await _context.PersonalPost
            .Include(p => p.Posts.MediaContent)
            .Where(p => p.PostedToId == userId && p.Posts.MediaContent != null)
            .OrderByDescending(_ => _.Posts.DateOfPost)
            .AsNoTracking()
            .Select(p => new PostDto
            {
                PostAuthor = new PostAuthor(
                    p.Author.avatar,
                    p.Author.firstName,
                    p.Author.middleName,
                    p.Author.lastName,
                    p.Author.users.PublicId),
                Post = new Post(p.PostId, p.Posts.Token, p.Posts.PostContent, p.Posts.Likes, p.Posts.Dislikes, p.Posts.DateOfPost, p.Posts.LastModified, p.Posts.MediaContent),
                PostedToUserId = publicId,
            })
            .ToListAsync();

            return sortedItems;
        }

        public async Task<PersonalPost> GetPostByTokenAsync(string token)
        {
            var post = await _context.PersonalPost
                .Include(p => p.Posts)
                .FirstOrDefaultAsync(p => p.Posts.Token == token);
            if (post == null) return null;
            return post;
        }


        public async Task<Post> GetPostWithReactionsByTokenAsync(string token)
        {
            var post = await _context.Post
                .Include(p => p.PostReactions)
                .FirstOrDefaultAsync(p => p.Token == token);
            if (post == null) return null;
            return post;
        }

        public async Task<Post> Create(CreatePostDto postDto, Personal author, int postedToUserId )
        {
            Post newPost = new(postDto.Message);

            await InsertSaveAsync(newPost);

            //Create new junction table with user and postId
            PersonalPost personalPost = new PersonalPost()
            {
                AuthorId = author.id,
                PostId = newPost.Id,
                PostedToId = postedToUserId, 
            };


            await InsertSaveAsync(personalPost);

            return newPost;
        }

        public async Task UploadFile(FileUpload newData, Post dto)
        {
            bool isVideo = FileHandlerService.FormatIsVideo(newData.Type);
            bool isImage = FileHandlerService.FormatIsImage(newData.Type);
            if (isVideo || isImage)
            {
                MediaContent media = new(dto.Id, newData.Name, newData.Type, newData.File.Length); //mentés az adatbázisba
                
                var name = await _storageRepository.AddFile(newData, BucketSelector.IMAGES_BUCKET_NAME); //Csak a fájl neve tér vissza
                media.FileName = name; //Egyelőre felülírjuk, de lehetne originalName a fájlhoz.
                await InsertSaveAsync(media);
            }
        }

        public async Task RemovePostAsync(Post post)
        {
            var personalPost = await _context.PersonalPost.FirstOrDefaultAsync(p => p.PostId == post.Id);
            if (personalPost != null)
            {
                await RemoveAsync<PersonalPost>(personalPost);
            }
            await RemoveAsync<Post>(post);
            await SaveAsync();
        }

        public async Task LikePost(int postId, int userId, ReactionType reactionType)
        {
            PostReaction reaction = new(postId, userId, reactionType);
            _context.PostReaction.Add(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task DislikePost(int postId, int userId, ReactionType reactionType)
        {
            PostReaction reaction = new(postId, userId, reactionType);
            _context.PostReaction.Add(reaction);
            await _context.SaveChangesAsync();
        }

    }
}
