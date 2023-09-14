namespace KozoskodoAPI.DTOs
{
    public class SignUpDto
    {
        public string firstName { get; set; } = string.Empty;
        public string? middleName { get; set; }
        public string lastName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public DateTime birthDay { get; set; }
    }
}
