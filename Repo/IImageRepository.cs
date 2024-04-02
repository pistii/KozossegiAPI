using KozoskodoAPI.Controllers;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IImageRepository
    {
        Task<IActionResult> GetPostImage(int postId);
        Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9);
        Task<IActionResult> Upload([FromForm] AvatarUpload fileUpload);
    }
}
