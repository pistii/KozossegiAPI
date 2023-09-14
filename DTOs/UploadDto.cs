namespace KozoskodoAPI.DTOs
{
    public class UploadDto
    {
        public int userId {  get; set; }
        public byte[] img { get; set; }
        public string token { get; set; }
    }
}
