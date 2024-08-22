namespace KozossegiAPI.Models.Cloud
{
    public class AvatarUpload : FileUpload
    {
        public AvatarUpload(int UserId, string name, string type, IFormFile file) : base(name, type, file)
        {
            this.UserId = UserId;
        }

        public AvatarUpload() { }


        public int UserId { get; set; }

    }
}
