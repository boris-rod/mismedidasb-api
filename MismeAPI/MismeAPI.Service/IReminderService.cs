using MismeAPI.Common.DTO.Request.Reminder;
using MismeAPI.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IReminderService
    {
        Task ChangeReminderTranslationAsync(int loggedUser, ReminderTranslationRequest reminderTranslationRequest, int id);

        Task<IEnumerable<Reminder>> GetRemindersAdminAsync(int loggedUser);
    }
}