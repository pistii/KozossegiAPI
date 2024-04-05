namespace KozossegiAPI.SMTP
{
    public interface IMailSender
    {
        public void SendMail(string subject, string content, string full_name, string email);
    }
}
