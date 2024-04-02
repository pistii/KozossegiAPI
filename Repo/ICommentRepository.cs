using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface ICommentRepository<T> : IHelperRepository<T>
    {
        Task<Post?> GetPostWithComments(int postId);
    }
}
