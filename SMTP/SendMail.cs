using Google.Api;
using KozossegiAPI.SMTP;
using KozossegiAPI.SMTP.Helpers;
using MailKit.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Net;
using System.Security.Policy;
using System.Web;

namespace KozoskodoAPI.SMTP
{
    public class SendMail : IMailSender
    {
        private readonly AppSettings _appSettings;

        public SendMail(IOptions<AppSettings> appsettings)
        {
            _appSettings = appsettings.Value;

            //if (string.IsNullOrEmpty(_appSettings.Key) || 
            //    string.IsNullOrEmpty(_appSettings.Email) ||
            //    string.IsNullOrEmpty(_appSettings.Host)
            //    )
            //{
            //    throw new Exception("No email configuration found.");
            //}
        }

        void IMailSender.SendMail(string subject, string content, string full_name, string email)
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
                smtpClient.Connect(_appSettings.Host, _appSettings.Port, SecureSocketOptions.StartTlsWhenAvailable);
                smtpClient.Authenticate(_appSettings.UserName, _appSettings.Key);
                smtpClient.Send(mailMessage);
                smtpClient.Disconnect(true);
            }
        }
    }
}
