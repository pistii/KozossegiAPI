using KozoskodoAPI.Models;

namespace KozoskodoAPI.Repo
{
    public interface ICommentRepository<T> : IGenericRepository<T>
    {
        Task<Post?> GetPostWithComments(int postId);
    }
}
