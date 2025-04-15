namespace KozossegiAPI.Models.Cloud
{
    public class FileUpload
    {
        public FileUpload()
        {
            
        }

        public FileUpload(string name, string type, IFormFile file, long size)
        {
            this.Name = name;
            this.Type = type;
            this.File = file;
            this.Size = size;
        }

        public FileUpload(string name, string type, IFormFile file)
        {
            this.Name = name;
            this.Type = type;
            this.File = file;
        }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public IFormFile? File { get; set; }
        public long Size { get; set; }
    }
}
