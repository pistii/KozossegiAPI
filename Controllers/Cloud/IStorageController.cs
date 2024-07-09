using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;

namespace KozossegiAPI.Controllers.Cloud
{
    public interface IStorageController 
    {
        public Task<IActionResult> GetFile(string fileName, BucketSelector selectedBucket);
        public Task<string> AddFile([FromForm] FileUpload fileUpload, BucketSelector selectedBucket);
        Task<byte[]> GetFileAsByte(string fileName, BucketSelector selectedBucket);
        Task<IActionResult> GetVideoChunk(string fileName, long rangeStart, long rangeEnd);
        Task<IActionResult> GetVideo(string fileName);
        Task<byte[]> GetVideoChunkBytes(string fileName, long rangeStart, long rangeEnd);
    }
}
