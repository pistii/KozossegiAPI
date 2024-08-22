namespace KozossegiAPI.SMTP
{
    public interface IMailSender
    {
        string getEmailTemplate(string fileName);
        void SendEmail(string subject, string content, string full_name, string email);
    }
}
