using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailResponseAsync(string subject, string htmlMessage, IEnumerable<string> email);
    }
}
