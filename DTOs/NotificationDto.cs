namespace KozoskodoAPI.DTOs
{
    public class NotificationDto
    {
        public int applicantId {  get; set; }
        public int toUserId { get; set; }
        public string content { get; set; } = string.Empty;
    }
}
