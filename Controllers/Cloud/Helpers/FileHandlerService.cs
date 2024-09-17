using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace KozossegiAPI.Controllers.Cloud.Helpers
{
    public class FileHandlerService
    {
        //Sizes in bytes
        const uint IMAGES_MAX_SIZE = 150_000_000; //15mb
        const uint VIDEOS_MAX_SIZE = 512_000_000; //512mb
        const uint AUDIO_MAX_SIZE = 300_000_000; // 30mb

        public uint GetMaxAudioSize
        {
            get { return AUDIO_MAX_SIZE; }
        }
        public uint GetMaxVideoSize
        {
            get { return VIDEOS_MAX_SIZE; }
        }
        public uint GetMaxImageSize
        {
            get { return IMAGES_MAX_SIZE; }
        }

        static List<string> imageFormats = new List<string>()
            {
                "image/png", "image/jpg", "image/jpeg", "image/gif", "image/bmp"
            };
        static List<string> videoFormats = new List<string>()
            {
            "video/mp4"
        };
        static List<string> audioFormats = new List<string>()
        {
            "audio/wav"
        };

        public static string UploadFile(IFormFile file, string fileName, string fileType, BucketSelector bucketName)
        {
            if (!FormatIsValid(fileType) && !FileSizeCorrect(file, fileType))
            {
                return null;
            }

            return null;
        }

        public static bool FileSizeCorrect(IFormFile file, string fileType)
        {
            long size = file.Length;
            if (imageFormats.Contains(fileType))
            {
                if (size <= IMAGES_MAX_SIZE)
                    return true;
            }
            else if (audioFormats.Contains(fileType))
            {
                if (size <= AUDIO_MAX_SIZE)
                    return true;
            }
            else if (videoFormats.Contains(fileType))
            {
                if (size <= VIDEOS_MAX_SIZE)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <returns>Returns true if file format is accepted and valid.</returns>
        public static bool FormatIsValid(string format)
        {
            if (audioFormats.Contains(format) || videoFormats.Contains(format) || imageFormats.Contains(format))
                return true;
            return false;
        }

        public static bool FormatIsVideo(string format)
        {
            return videoFormats.Contains(format);
        }

        public static bool FormatIsImage(string format)
        {
            return imageFormats.Contains(format);
        }

        public static bool FormatIsAudio(string format)
        {
            return audioFormats.Contains(format);
        }
    }
}
