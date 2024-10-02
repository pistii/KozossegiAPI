using KozossegiAPI.DTOs;
using KozossegiAPI.Models;
using KozossegiAPI.Models.Cloud;

namespace KozossegiAPI.Interfaces
{
    public interface IPostRepository<T1> : IGenericRepository<T1>
    {
        Task<ContentDto<PostDto>> GetAllPost(int profileId, int userId, int currentPage = 1, int itemPerRequest = 10);
        Task<Post> Create(CreatePostDto postDto);
        Task UploadFile(FileUpload mediaContent, Post postdto);
        Task<Post> GetPostByTokenAsync(string token);
        Task<Post> GetPostWithReactionsByTokenAsync(string token);
        Task<List<PostDto>> GetImagesAsync(int userId);
        Task RemovePostAsync(Post post);
        Task LikePost(ReactionDto postReaction, Post post, user user);
        Task DislikePost(ReactionDto postReaction, Post post, user user);
    }
}
