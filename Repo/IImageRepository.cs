using KozoskodoAPI.Controllers;
using KozoskodoAPI.Controllers.Cloud;
using KozoskodoAPI.DTOs;
using KozoskodoAPI.Models;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;

namespace KozoskodoAPI.Repo
{
    public interface IImageRepository
    {
        Task<MediaContent> GetPostImage(int postId);
        Task<List<PostDto>> GetAll(int userId, int currentPage = 1, int requestItems = 9);
        Task UpdateDatabaseImageUrl(int userId, string url);
    }
}
