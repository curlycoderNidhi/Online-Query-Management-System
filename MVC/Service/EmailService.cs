using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace MVC.Service
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSection = _config.GetSection("Smtp");

            var email = new MimeMessage();

            email.From.Add(new MailboxAddress(
                smtpSection["FromName"],
                smtpSection["Username"]
            ));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            email.Body = new TextPart("html")
            {
                Text = body
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpSection["Host"],
                int.Parse(smtpSection["Port"]),
                MailKit.Security.SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                smtpSection["Username"],
                smtpSection["Password"]
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
        
    }
}