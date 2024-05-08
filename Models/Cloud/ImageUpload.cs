namespace KozossegiAPI.Models.Cloud
{
    public class ImageUpload : FileUpload
    {
        public ImageUpload(string name, string type, IFormFile file) : base(name, type, file)
        {
        }

        public int UserId { get; set; }
        public string Description { get; set; }
        public string ImageType { get; set; } // profile, social
        public bool IsPublic { get; set; } = true;
        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsModified { get; set; }
    }
}
