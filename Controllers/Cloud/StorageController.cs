using Google.Cloud.Storage.V1;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Owin.Security.Provider;
using System;
using static KozoskodoAPI.Controllers.Cloud.StorageController;

namespace KozoskodoAPI.Controllers.Cloud
{
    public class StorageController : ControllerBase, IStorageController
    {
        public readonly DBContext _context;
        protected readonly string BASE_URL = "https://storage.googleapis.com/";
        protected readonly string AVATAR_BUCKET_NAME = "socialstream";
        protected readonly string IMAGES_BUCKET_NAME = "pb_imgs";


        public StorageController(DBContext context)
        {
            _context = context;
        }

        public string Url { get; set; } = string.Empty;

        
        public async Task<IActionResult> GetFile(string fileName, BucketSelector selectedBucket)
        {
            var client = StorageClient.Create();
            var stream = new MemoryStream(); //Stream will be updated on download just need an empty one to store the data
            var obj = await client.DownloadObjectAsync(GetSelectedBucketName(selectedBucket), fileName, stream);
            stream.Position = 0;

            return File(stream, obj.ContentType, obj.Name);
        }

        public async Task<string> AddFile(FileUpload fileUpload, BucketSelector selectedBucket)
        {
            var client = StorageClient.Create();
            Google.Apis.Storage.v1.Data.Object obj; 
            using (Stream stream = fileUpload.File.OpenReadStream())
            {
                obj = await client.UploadObjectAsync(GetSelectedBucketName(selectedBucket), Guid.NewGuid().ToString(), fileUpload.Type, stream);
            }

            //await UpdateDatabaseImageUrl(fileUpload, BASE_URL + AVATAR_BUCKET_NAME + obj.Name);
            return obj.Name;
        }

        /// <summary>
        /// Returns the name of the selected bucket
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public string GetSelectedBucketName(BucketSelector selector)
        {
            return selector == BucketSelector.AVATAR_BUCKET_NAME ? AVATAR_BUCKET_NAME : IMAGES_BUCKET_NAME;
        }
    }
}
