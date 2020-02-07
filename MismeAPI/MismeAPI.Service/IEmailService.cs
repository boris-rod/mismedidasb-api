using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IEmailService
    {
        Task SendEmailResponseAsync(string subject, string htmlMessage, string email);
    }
}