using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using AssetWeb.Models.Domain;
using Microsoft.Extensions.Options;
using System.Net;

namespace AssetWeb.Services
{
    public class EmailService
    {
        private readonly EmailSettings emailSettings;

        public EmailService(IOptions<EmailSettings> _emailSettings)
        {
            this.emailSettings = _emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MailMessage
            {
                From = new MailAddress(emailSettings.FromEmail, emailSettings.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var smtpClient = new SmtpClient
            {
                Host = emailSettings.Host,
                Port = emailSettings.Port,
                EnableSsl = emailSettings.EnableSsl,
                Credentials = new NetworkCredential(emailSettings.UserName, emailSettings.Password)
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}