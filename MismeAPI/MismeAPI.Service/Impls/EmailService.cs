using Microsoft.Extensions.Configuration;
using MismeAPI.Common;
using MismeAPI.Common.Exceptions;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
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

        public async Task SendEmailResponseAsync(string subject, string message, string email)
        {
            var apiKey = _configuration.GetValue<string>("Sendgrid:SendGridKey");
            string sandBoxMode = _configuration.GetValue<string>("Sendgrid:UseSandbox");
            string userSendgrid = _configuration.GetValue<string>("Sendgrid:SendGridUser");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(userSendgrid);
            var to = new EmailAddress(email);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);

            var response = await client.SendEmailAsync(msg);

            if (response.StatusCode.ToString() != "Accepted")
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Email");
            }
        }
    }
}