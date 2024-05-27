namespace KozossegiAPI.Models.Cloud
{
    public class AvatarUpload : FileUpload
    {
        public int UserId { get; set; }

        //public AvatarUpload(int UserId, string Name, IFormFile File)
        //{
        //    this.UserId = UserId;
        //    this.Name = Name;
        //    this.File = File;
        //}

        public AvatarUpload(int UserId, string name, string type, IFormFile file) : base(name, type, file)
        {
            this.UserId = UserId;
        }
    }
}
