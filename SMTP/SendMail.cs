using KozossegiAPI.Models;
using KozossegiAPI.SMTP.Helpers;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;

namespace KozossegiAPI.SMTP
{
    public class SendMail : IMailSender
    {
        private readonly AppSettings _appSettings;

        public SendMail(IOptions<AppSettings> appsettings)
        {
            _appSettings = appsettings.Value;

            if (string.IsNullOrEmpty(_appSettings.Email) ||
                string.IsNullOrEmpty(_appSettings.Server) ||
                _appSettings.Port == 0 ||
                _appSettings.SSL == 0
                )
            {
                throw new Exception("No email configuration found.");
            }
        }

        public void SendEmail(string subject, string content, string full_name, string email)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress("SocialStream", _appSettings.Email));
            mailMessage.To.Add(new MailboxAddress(full_name, email));
            mailMessage.Subject = $"{subject}";
            
            mailMessage.Body = new TextPart("html")
            {
                Text = $"{content}"
            };

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Connect(_appSettings.Server, _appSettings.Port, SecureSocketOptions.StartTlsWhenAvailable);
                smtpClient.Authenticate(_appSettings.Email, _appSettings.Password);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }
        }

        public string getEmailTemplate(string fileName)
        {
            string fullpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "templates");
            string templatePath = Path.Combine(fullpath, fileName);
            return System.IO.File.ReadAllText(templatePath);
        }

        public void UserDataChangedEmail(Personal user)
        {
            return;

            string fullName = user.middleName == null ? user.firstName + " " + user.lastName : user.firstName + " " + user.middleName + " " + user.lastName;

            var htmlTemplate = getEmailTemplate("userDataChanged.html");
            htmlTemplate = htmlTemplate.Replace("{userFullName}", user.lastName);

            SendEmail("Módosítás történt a felhasználói fiókodban", htmlTemplate, fullName, user.users.email);

        }

    }
}
