using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Services.Impls
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task SendEmailResponseAsync(string subject, string message, IEnumerable<string> emails)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(_configuration.GetValue<string>("SMTP:Server"), int.Parse(_configuration.GetValue<string>("SMTP:Port")), false);

                    await client.AuthenticateAsync(_configuration.GetValue<string>("SMTP:User"), _configuration.GetValue<string>("SMTP:Password"));

                    var mess = new MimeMessage();

                    mess.From.Add(new MailboxAddress(_configuration.GetValue<string>("SMTP:From")));
                    foreach (var email in emails)
                    {
                        mess.To.Add(new MailboxAddress(email));
                    }
                    mess.Subject = subject;
                    mess.Body = new TextPart("html")
                    {
                        Text = message
                    };

                    await client.SendAsync(mess);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendEmailResponseWithAttachmentAsync(string subject, string message, IEnumerable<string> emails, string filePath)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    await client.ConnectAsync(_configuration.GetValue<string>("SMTP:Server"), int.Parse(_configuration.GetValue<string>("SMTP:Port")), false);

                    await client.AuthenticateAsync(_configuration.GetValue<string>("SMTP:User"), _configuration.GetValue<string>("SMTP:Password"));

                    var mess = new MimeMessage();

                    mess.From.Add(new MailboxAddress(_configuration.GetValue<string>("SMTP:From")));
                    foreach (var email in emails)
                    {
                        mess.To.Add(new MailboxAddress(email));
                    }
                    mess.Subject = subject;

                    var builder = new BodyBuilder();

                    builder.TextBody = message;

                    builder.Attachments.Add(filePath);

                    mess.Body = builder.ToMessageBody();

                    await client.SendAsync(mess);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}