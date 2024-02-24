using Google.Cloud.Storage.V1;
using KozoskodoAPI.Data;
using KozoskodoAPI.Models;
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
                obj = await client.UploadObjectAsync(GetSelectedBucketName(selectedBucket), GenerateUniqueObjectName(), fileUpload.Type, stream);
            }

            //await UpdateDatabaseImageUrl(fileUpload, BASE_URL + AVATAR_BUCKET_NAME + obj.Name);
            return obj.Name;
        }

        /// <summary>
        /// Generál egy tokent az aktuális dátum alapján.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GenerateUniqueObjectName()
        {
                DateTime now = DateTime.Now;
                string token = "";

                Random random = new Random();
                List<int> dates = new List<int>()
                {
                    now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond
                };

                for (int i = 0; i < dates.Count(); i++)
                {
                    char randomChar = (char)random.Next('a', 'z' + 1);
                    string salt = string.Empty;

                    if (dates[i] == now.Year)
                    {
                        for (int s = 0; s < 3; s++)
                        {
                            salt += GenerateRandomChar(1, 4) + s;
                        }
                        token += salt;
                    }
                    else if (dates[i] == now.Millisecond)
                    {
                    token += $"{dates[i]:D4}{GenerateRandomChar(0, 2)}";
                    }
                    else
                    {
                        token += $"{GenerateRandomChar(random.Next(0, 2), 3)}{dates[i]:D2}{GenerateRandomChar(random.Next(0, 2), 3)}";
                    }
                    token += randomChar;
                }

                return token;
        }

        public static string GenerateRandomChar(int min, int max) 
        {
            Random random = new Random();
            string randomString = string.Empty;
            for (int i = min; i < max; i++)
            {
                randomString += (char)random.Next('a', 'z' + 1);
            }
            return randomString;
        }


        public enum BucketSelector
        {
            AVATAR_BUCKET_NAME = 0,
            IMAGES_BUCKET_NAME = 1,
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


        public class FileUpload
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public IFormFile? File { get; set; }
    }

    public class ImageUpload : FileUpload
    {
        public int UserId { get; set; }
        public string Description { get; set; }
        public string ImageType { get; set; } // profile, social
        public bool IsPublic { get; set; } = true;
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }

    public class AvatarUpload : FileUpload
    {
        public int UserId { get; set; }

        public AvatarUpload(int UserId, string Name, IFormFile File)
        {
            this.UserId = UserId;
            this.Name = Name;
            this.File = File;
        }

        public AvatarUpload()
        {
            
        }
    }

    public interface IStorageController
    {
        public Task<IActionResult> GetFile(string fileName, BucketSelector selectedBucket);
        public Task<string> AddFile([FromForm] FileUpload fileUpload, BucketSelector selectedBucket);
    }

}
