using Google.Apis.Storage.v1;
using Google.Cloud.Storage.V1;
using KozoskodoAPI.Data;
using KozossegiAPI.Controllers.Cloud;
using KozossegiAPI.Controllers.Cloud.Helpers;
using KozossegiAPI.Models.Cloud;
using KozossegiAPI.Storage;
using Microsoft.AspNetCore.Mvc;
namespace KozoskodoAPI.Controllers.Cloud
{
    public class StorageRepository : ControllerBase, IStorageRepository
    {
        protected readonly string BASE_URL = "https://storage.googleapis.com/";
        protected readonly Dictionary<BucketSelector, string> bucketUrls;
        private IChatStorage _chatStorage;

        public StorageRepository(IChatStorage chatStorage)
        {   
            _chatStorage = chatStorage;
            bucketUrls = new Dictionary<BucketSelector, string>
            {
                { BucketSelector.AVATAR_BUCKET_NAME, "socialstream" },
                { BucketSelector.IMAGES_BUCKET_NAME, "pb_imgs" },
                { BucketSelector.CHAT_BUCKET_NAME, "socialstream_chat" }
            };
        }

        public string Url { get; set; } = string.Empty;


        /// <summary>
        /// Send request towards cloud
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="selectedBucket"></param>
        /// <returns>Returns the selected file as IActionResult</returns>
        public async Task<IActionResult> GetFile(string fileName, BucketSelector selectedBucket)
        {
            var client = StorageClient.Create();
            var stream = new MemoryStream(); //Stream will be updated on download just need an empty one to store the data
            var obj = await client.DownloadObjectAsync(GetSelectedBucketName(selectedBucket), fileName, stream);
            stream.Position = 0;

            return File(stream, obj.ContentType, obj.Name);
            
        }

        public async Task<byte[]> GetFileAsByte(string fileName, BucketSelector selectedBucket)
        {
            var client = StorageClient.Create();
            using (var stream = new MemoryStream())
            {
                var obj = await client.DownloadObjectAsync(GetSelectedBucketName(selectedBucket), fileName, stream);
                stream.Position = 0;
                
                var fileByte = stream.ToArray();
                return fileByte;
            }
        }

        public async Task<string> AddFile(FileUpload fileUpload, BucketSelector selectedBucket)
        {
            var client = StorageClient.Create();
            Google.Apis.Storage.v1.Data.Object obj;
            using (Stream stream = fileUpload.File.OpenReadStream())
            {
                obj = await client.UploadObjectAsync(GetSelectedBucketName(selectedBucket), Guid.NewGuid().ToString(), fileUpload.Type, stream);
            }
            return obj.Name;
        }


        public async Task<IActionResult> GetVideoChunk(string fileName, long rangeStart, long rangeEnd)
        {
            var file = await GetFileAsByte(fileName, BucketSelector.CHAT_BUCKET_NAME);

            if (file.Length == 0)
                return NotFound();

            var contentLength = rangeEnd - rangeStart + 1;
            var buffer = new byte[contentLength];

            using (var memoryStream = new MemoryStream(file))
            {
                memoryStream.Seek(rangeStart, SeekOrigin.Begin);
                await memoryStream.ReadAsync(buffer, 0, buffer.Length);
            }

            Response.StatusCode = 206; // Partial Content
            Response.Headers["Content-Range"] = $"bytes={rangeStart}-{rangeEnd}/{file.Length}";

            return File(buffer, "video/mp4", enableRangeProcessing: true);
        }

        public async Task<byte[]> GetVideoChunkBytes(string fileName, long rangeStart, long rangeEnd)
        {
            var file = await GetFileAsByte(fileName, BucketSelector.CHAT_BUCKET_NAME);

            if (file.Length == 0)
                return null;

            //using (var memoryStream = new MemoryStream(file))
            //{
            //    var contentLength = rangeEnd - rangeStart + 1;
            //    var buffer = new byte[contentLength];

            //    memoryStream.Seek(rangeStart, SeekOrigin.Begin);
            //    await memoryStream.ReadAsync(buffer, 0, buffer.Length);

            //    var response = new FileContentResult(buffer, "video/mp4")
            //    {
            //        FileDownloadName = fileName,
            //        EnableRangeProcessing = true
            //    };

            //    Response.Headers["Content-Range"] = $"bytes {rangeStart}-{rangeEnd}/{file.Length}";
            //    Response.Headers["Accept-Ranges"] = "bytes";
            //    Response.Headers["Content-Length"] = buffer.Length.ToString();

            //    return File(buffer, "video/mp4", true);
            //}


            var contentLength = rangeEnd - rangeStart + 1;
            var buffer = new byte[contentLength];

            using (var memoryStream = new MemoryStream(file))
            {
                memoryStream.Seek(rangeStart, SeekOrigin.Begin);
                await memoryStream.ReadAsync(buffer, 0, buffer.Length);
            }

            
            return buffer;
        }

        
        [HttpGet("video/{fileName}")]
        public async Task<IActionResult> GetVideo(string fileName)
        {
            if (Request == null)
            {
                var file = _chatStorage.GetValue(fileName); //Return file from cache if available
                if (file == null)
                {
                    file = await GetFileAsByte(fileName, BucketSelector.CHAT_BUCKET_NAME); //Get file from cloud and store in cache
                    _chatStorage.Create(fileName, file);
                }
                return File(file, "video/mp4", enableRangeProcessing: true);
            }
            if (Request.Headers.ContainsKey("Range")) //In case if it's range request
            {
                var rangeHeader = Request.Headers["Range"].ToString();
                var range = rangeHeader.Split(new[] { '=', '-' });
                var rangeStart = long.Parse(range[1]);
                var rangeEnd = range.Length > 2 && long.TryParse(range[2], out long end) ? end : rangeStart + 1_000_000 - 1;

                return await GetVideoChunk(fileName, rangeStart, rangeEnd);
            }
            return NotFound();
        }

        /// <summary>
        /// Returns the name of the selected bucket
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public string GetSelectedBucketName(BucketSelector selector)
        {
            if (bucketUrls.ContainsKey(selector))
            {
                return bucketUrls[selector];
            }
            else
            {
                throw new Exception("Bucket not found for selector: " + selector);
            }
        }
    }
}
