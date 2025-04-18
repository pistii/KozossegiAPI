using KozossegiAPI.Models.Cloud;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;

namespace KozossegiAPI.Controllers.Cloud.Helpers
{
    public static class FileHandlerService
    {
        //Sizes in bytes
        const uint IMAGES_MAX_SIZE = 150_000_000; //15mb
        const uint VIDEOS_MAX_SIZE = 512_000_000; //512mb
        const uint AUDIO_MAX_SIZE = 300_000_000; // 30mb

        public static uint GetMaxAudioSize
        {
            get { return AUDIO_MAX_SIZE; }
        }
        public static uint GetMaxVideoSize
        {
            get { return VIDEOS_MAX_SIZE; }
        }
        public static uint GetMaxImageSize
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

        public static bool FileSizeCorrect(IFormFile file, string fileType)
        {
            long size = file.Length;
            return fileType switch
            {
                var type when imageFormats.Contains(type) && size <= IMAGES_MAX_SIZE => true,
                var type when audioFormats.Contains(type) && size <= AUDIO_MAX_SIZE => true,
                var type when videoFormats.Contains(type) && size <= VIDEOS_MAX_SIZE => true,
                _ => false
            };
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
