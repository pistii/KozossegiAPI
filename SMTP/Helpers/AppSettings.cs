namespace KozossegiAPI.SMTP.Helpers
{
    public class AppSettings
    {

        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }

        public int SSL { get; set; }
    }
}
