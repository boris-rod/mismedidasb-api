using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.ContactUs;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ContactUsService : IContactUsService
    {
        private readonly IUnitOfWork _uow;

        public ContactUsService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<ContactUs> ChangeImportantStatusAsync(int loggedUser, int id, bool important)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var contactUs = await _uow.ContactUsRepository.GetAll().Where(c => c.Id == id)
                .Include(c => c.User)
                .FirstOrDefaultAsync();

            if (contactUs == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Contact Us");
            }
            contactUs.Priority = important == true ? ContactPriorityEnum.IMPORTANT : ContactPriorityEnum.NORMAL;
            _uow.ContactUsRepository.Update(contactUs);
            await _uow.CommitAsync();
            return contactUs;
        }

        public async Task<ContactUs> ChangeReadStatusAsync(int loggedUser, int contactUsId, bool read)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            var contactUs = await _uow.ContactUsRepository.GetAll().Where(c => c.Id == contactUsId)
                .Include(c => c.User)
                .FirstOrDefaultAsync();

            if (contactUs == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Contact Us");
            }
            contactUs.Read = read;
            _uow.ContactUsRepository.Update(contactUs);
            await _uow.CommitAsync();
            return contactUs;
        }

        public async Task<ContactUs> CreateContactUsAsync(int userId, ContactUsRequest request)
        {
            var user = await _uow.UserRepository.GetAsync(userId);

            if (user == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "User");
            }

            var contact = new ContactUs();
            contact.Body = request.Body;
            contact.Subject = request.Subject;
            contact.UserId = userId;
            contact.User = user;
            contact.Priority = ContactPriorityEnum.NORMAL;
            contact.Read = false;
            contact.CreatedAt = DateTime.UtcNow;

            await _uow.ContactUsRepository.AddAsync(contact);
            await _uow.CommitAsync();
            return contact;
        }

        public async Task<PaginatedList<ContactUs>> GetContactsAsync(int loggedUser, int pag, int perPag, string sortOrder, int prioriF, int readF, string search)
        {
            var user = await _uow.UserRepository.GetAsync(loggedUser);
            if (user.Role == RoleEnum.NORMAL)
            {
                throw new NotAllowedException("User");
            }

            var result = _uow.ContactUsRepository.GetAll()
                .Include(c => c.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(
                        i => i.User.FullName.ToLower().Contains(search.ToLower()) ||
                             i.User.Email.ToLower().Contains(search.ToLower()) ||
                             i.Subject.ToLower().Contains(search.ToLower()));
            }

            // define read status filter
            if (readF > -1)
            {
                result = result.Where(i => readF == 0 ? i.Read == false : i.Read == true);
            }

            // define priority status filter
            if (prioriF > -1)
            {
                result = result.Where(i => prioriF == 0 ? i.Priority == ContactPriorityEnum.NORMAL : i.Priority == ContactPriorityEnum.IMPORTANT);
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "userName_desc":
                        result = result.OrderByDescending(i => i.User.FullName);
                        break;

                    case "userName_asc":
                        result = result.OrderBy(i => i.User.FullName);
                        break;

                    case "userEmail_desc":
                        result = result.OrderByDescending(i => i.User.Email);
                        break;

                    case "userEmail_asc":
                        result = result.OrderBy(i => i.User.Email);
                        break;

                    case "subject_desc":
                        result = result.OrderByDescending(i => i.Subject);
                        break;

                    case "subject_asc":
                        result = result.OrderBy(i => i.Subject);
                        break;

                    //case "priority_desc":
                    //    result = result.OrderByDescending(i => i.Priority.ToString());
                    //    break;

                    //case "priority_asc":
                    //    result = result.OrderBy(i => i.Priority.ToString());
                    //    break;

                    case "createdAt_desc":
                        result = result.OrderByDescending(i => i.CreatedAt);
                        break;

                    case "createdAt_asc":
                        result = result.OrderBy(i => i.CreatedAt);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<ContactUs>.CreateAsync(result, pag, perPag);
        }
    }
}