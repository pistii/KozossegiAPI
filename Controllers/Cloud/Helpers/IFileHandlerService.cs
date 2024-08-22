namespace KozossegiAPI.Controllers.Cloud.Helpers
{
    public interface IFileHandlerService
    {
        bool FormatIsValid(string format);
        bool FormatIsVideo(string format);
        bool FormatIsImage(string format);
        bool FormatIsAudio(string format);
        string UploadFile(IFormFile file, string fileName, string fileType, BucketSelector bucketName);
        bool FileSizeCorrect(IFormFile file, string fileType);
    }
}
