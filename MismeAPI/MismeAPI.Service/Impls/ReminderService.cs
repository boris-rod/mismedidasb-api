using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Reminder;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ReminderService : IReminderService
    {
        private readonly IUnitOfWork _uow;

        public ReminderService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task ChangeReminderTranslationAsync(int loggedUser, ReminderTranslationRequest reminderTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var reminder = await _uow.ReminderRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (reminder == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Reminder");
            }

            switch (reminderTranslationRequest.Lang)
            {
                case "en":
                    reminder.TitleEN = reminderTranslationRequest.Title;
                    reminder.BodyEN = reminderTranslationRequest.Body;
                    break;

                case "it":
                    reminder.TitleIT = reminderTranslationRequest.Title;
                    reminder.BodyIT = reminderTranslationRequest.Body;
                    break;

                default:
                    reminder.Title = reminderTranslationRequest.Title;
                    reminder.Body = reminderTranslationRequest.Body;
                    break;
            }

            _uow.ReminderRepository.Update(reminder);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Reminder>> GetRemindersAdminAsync(int loggedUser)
        {
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var reminders = await _uow.ReminderRepository.GetAll().ToListAsync();
            return reminders;
        }
    }
}