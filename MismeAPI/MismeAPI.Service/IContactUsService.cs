using MismeAPI.Common.DTO.Request.ContactUs;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IContactUsService
    {
        Task<ContactUs> CreateContactUsAsync(int userId, ContactUsRequest request);

        Task<ContactUs> ChangeReadStatusAsync(int loggedUser, int contactUsId, bool read);

        Task<ContactUs> ChangeImportantStatusAsync(int loggedUser, int id, bool important);

        Task<PaginatedList<ContactUs>> GetContactsAsync(int loggedUser, int pag, int perPag, string sortOrder, int prioriF, int readF, string search);

        Task AnswerMmessageAsync(int loggedUser, ContactAnswerRequest answerRequest);
    }
}