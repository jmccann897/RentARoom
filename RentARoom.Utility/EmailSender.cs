using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // useful set up instruction https://www.youtube.com/watch?v=JX2bjkxnxTo and MS docs: https://learn.microsoft.com/en-gb/aspnet/core/security/authentication/accconfirm?view=aspnetcore-9.0&tabs=visual-studio

            // Configure smtp server
            var serverEmail = _configuration.GetValue<string>("EmailConfiguration:Email");
            var password = _configuration.GetValue<string>("EmailConfiguration:Password");
            var host = _configuration.GetValue<string>("EmailConfiguration:Host");
            var port = _configuration.GetValue<int>("EmailConfiguration:Port");

            var smtpClient = new SmtpClient(host, port)
            { 
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(serverEmail, password)
            };

            // configure email
            var message = new MailMessage
            {
                From = new MailAddress(serverEmail!, "RentARoom"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
             message.To.Add(email);
            await smtpClient.SendMailAsync(message);
        }
    }
}
