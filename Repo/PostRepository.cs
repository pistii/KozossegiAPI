﻿using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Data;
using KozossegiAPI.DTOs;
using KozossegiAPI.Interfaces;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;
using Microsoft.EntityFrameworkCore;

namespace KozossegiAPI.Repo
{
    public class PostRepository : GenericRepository<PostDto>, IPostRepository<PostDto>
    {
        private readonly DBContext _context;
        private IStorageRepository _storageController;

        public PostRepository(
            DBContext context,
            IStorageRepository storageRepository
            ) : base( context )
        {
            _context = context;
            _storageController = storageRepository;
        }

        /// <summary>
        /// Get all post with comments
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="userId"></param>
        /// <param name="currentPage"></param>
        /// <param name="itemPerRequest"></param>
        /// <returns>PostDto as post with the comments and the media content.</returns>
        public async Task<ContentDto<PostDto>> GetAllPost(int toId, int authorId, int currentPage = 1, int itemPerRequest = 10)
        {
            var sortedItems = await _context.PersonalPost
            .Include(p => p.Posts.MediaContent)
            .Include(c => c.Posts.PostComments)
            .Include(p => p.Posts.PostReactions)
            .Where(p => p.PostedToId == toId)
            .OrderByDescending(_ => _.Posts.DateOfPost)
            .AsNoTracking()
            .Select(p => new PostDto
            {
                PostAuthor = new PostAuthor(
                    p.Personal.avatar,
                    p.Personal.firstName,
                    p.Personal.middleName,
                    p.Personal.lastName,
                    p.Personal.id),
                Post = new Post(p.PostId, p.Posts.Token, p.Posts.PostContent, 
                p.Posts.PostReactions.Count(c => c.ReactionTypeId == 1), //like
                 p.Posts.PostReactions.Count(c => c.ReactionTypeId == 2), //dislike
                p.Posts.DateOfPost, p.Posts.MediaContent),
                PostedToUserId = toId,
                CommentsQty = p.Posts.PostComments.Count,
            })
            .ToListAsync();

            if (sortedItems == null) return null;
            int totalPages = await GetTotalPages(sortedItems, itemPerRequest);
            var returnValue = Paginator(sortedItems, currentPage, itemPerRequest).ToList();

            return new ContentDto<PostDto>(returnValue, totalPages);
        }

        public async Task<List<PostDto>> GetImagesAsync(int userId)
        {
            var sortedItems = await _context.PersonalPost
            .Include(p => p.Posts.MediaContent)
            .Where(p => p.PostedToId == userId && p.Posts.MediaContent != null)
            .OrderByDescending(_ => _.Posts.DateOfPost)
            .AsNoTracking()
            .Select(p => new PostDto
            {
                PostAuthor = new PostAuthor(
                    p.Personal.avatar,
                    p.Personal.firstName,
                    p.Personal.middleName,
                    p.Personal.lastName,
                    p.Personal.id),
                Post = new Post(p.PostId, p.Posts.Token, p.Posts.PostContent, p.Posts.Likes, p.Posts.Dislikes, p.Posts.DateOfPost, p.Posts.MediaContent),
                PostedToUserId = userId,
            })
            .ToListAsync();

            return sortedItems;
        }

        public async Task<Post> GetPostByTokenAsync(string token)
        {
            var post = await _context.Post
                .FirstOrDefaultAsync(p => p.Token == token);
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

        public async Task<Post> Create(CreatePostDto postDto)
        {
            Post newPost = new()
            {
                Token = Guid.NewGuid().ToString(),
                PostContent = postDto.post.PostContent
            };

            await InsertSaveAsync(newPost);

            //Create new junction table with user and postId
            PersonalPost personalPost = new PersonalPost()
            {
                AuthorId = postDto.PostAuthor.AuthorId,
                PostId = newPost.Id,
                PostedToId = postDto.PostedToUserId
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
                
                var name = await _storageController.AddFile(newData, BucketSelector.IMAGES_BUCKET_NAME); //Csak a fájl neve tér vissza
                media.FileName = name;
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

        public async Task LikePost(ReactionDto postReaction, Post post, user user)
        {
            PostReaction reaction = new()
            {
                PostId = post.Id,
                ReactionTypeId = 1,
                UserId = user.userID
            };
            _context.PostReaction.Add(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task DislikePost(ReactionDto postReaction, Post post, user user)
        {
            PostReaction reaction = new()
            {
                PostId = post.Id,
                ReactionTypeId = 2,
                UserId = user.userID
            };
            _context.PostReaction.Add(reaction);
            await _context.SaveChangesAsync();
        }
    }
}
