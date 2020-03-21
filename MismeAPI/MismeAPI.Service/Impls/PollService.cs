using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request;
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
    public class PollService : IPollService
    {
        private readonly IUnitOfWork _uow;

        public PollService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<Poll> CreatePollAsync(int loggedUser, CreatePollRequest poll)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename
            var existPollName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == poll.Name.ToLower());

            if (existPollName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Poll name");
            }
            var p = new Poll();
            p.CreatedAt = DateTime.UtcNow;
            p.ModifiedAt = DateTime.UtcNow; ;
            p.Name = poll.Name;
            p.Description = poll.Description;
            p.Codename = poll.Codename;
            p.ConceptId = poll.ConceptId;

            await _uow.PollRepository.AddAsync(p);
            await _uow.CommitAsync();

            return p;
        }

        public async Task DeletePollAsync(int loggedUser, int id)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAsync(id);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }

            _uow.PollRepository.Delete(pd);
            await _uow.CommitAsync();
        }

        public async Task<IEnumerable<Poll>> GetAllPollsAsync()
        {
            var result = await _uow.PollRepository.GetAll()
                .Include(p => p.Questions)
                .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Poll>> GetAllPollsByConceptAsync(int conceptId)
        {
            var result = await _uow.PollRepository.GetAll().Where(p => p.ConceptId == conceptId).ToListAsync();
            return result;
        }

        public async Task<Poll> GetPollByIdAsync(int id)
        {
            var poll = await _uow.PollRepository.GetAll()
                .Where(p => p.Id == id)
                .Include(p => p.Questions)
                .FirstOrDefaultAsync();
            if (poll == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            return poll;
        }

        public async Task SetPollResultAsync(int loggedUser, SetPollResultRequest pollResult)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAsync(pollResult.PollId);
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            var userPoll = new UserPoll();
            userPoll.PollId = pd.Id;
            userPoll.Result = pollResult.Result;
            userPoll.UserId = loggedUser;
            userPoll.CompletedAt = DateTime.UtcNow;
            await _uow.UserPollRepository.AddAsync(userPoll);
            await _uow.CommitAsync();
        }

        public async Task<Poll> UpdatePollDataAsync(int loggedUser, UpdatePollRequest poll)
        {
            // not found poll?
            var pd = await _uow.PollRepository.GetAll().Where(p => p.Id == poll.Id)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }

            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate codename, except itself
            var existName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == poll.Name.ToLower() && p.Id != poll.Id);
            if (existName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Name");
            }

            pd.ModifiedAt = DateTime.UtcNow;
            pd.Name = poll.Name;
            pd.Description = poll.Description;

            _uow.PollRepository.Update(pd);
            await _uow.CommitAsync();
            return pd;
        }

        public async Task<Poll> UpdatePollTitleAsync(int loggedUser, string title, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // not found poll?
            var pd = await _uow.PollRepository.GetAll().Where(p => p.Id == id)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();
            if (pd == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Poll");
            }
            // validate poll title
            var existPollName = await _uow.PollRepository.FindByAsync(p => p.Name.ToLower() == title.ToLower());

            if (existPollName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Poll name");
            }
            pd.Name = title;
            await _uow.PollRepository.UpdateAsync(pd, id);
            await _uow.CommitAsync();

            return pd;
        }
    }
}