using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailResponseAsync(string subject, string htmlMessage, IEnumerable<string> email);

        Task StoreEmailAync(string subject, string htmlMessage, IEnumerable<string> email, string filePath = "");

        Task ProcessScheduledEmailAsync(ScheduledEmail scheduledEmail);

        Task SendEmailResponseWithAttachmentAsync(string subject, string htmlMessage, IEnumerable<string> email, string filePath);
    }
}
