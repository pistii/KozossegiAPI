using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;

namespace KozossegiAPI.Controllers.Cloud
{
    public interface IStorageController
    {
        public Task<IActionResult> GetFile(string fileName, BucketSelector selectedBucket);
        public Task<string> AddFile([FromForm] FileUpload fileUpload, BucketSelector selectedBucket);
    }
}
