using Microsoft.EntityFrameworkCore;
using MismeAPI.Common;
using MismeAPI.Common.DTO.Request.Concept;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ConceptService : IConceptService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileService _fileService;

        public ConceptService(IUnitOfWork uow, IFileService fileService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task<Concept> AddConceptAsync(int loggedUser, AddConceptRequest concept)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            // validate concept title
            var existConceptName = await _uow.ConceptRepository.FindByAsync(p => p.Title.ToLower() == concept.Title.ToLower());
            if (existConceptName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Concept name");
            }

            var conceptDb = new Concept();
            conceptDb.Title = concept.Title;
            conceptDb.Description = concept.Description;
            conceptDb.CreatedAt = DateTime.UtcNow;
            conceptDb.ModifiedAt = DateTime.UtcNow;

            if (concept.Image != null)
            {
                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(concept.Image, guid);
                conceptDb.Image = guid;
            }
            await _uow.ConceptRepository.AddAsync(conceptDb);
            await _uow.CommitAsync();
            return conceptDb;
        }

        public async Task ChangeConceptPollOrderAsync(int loggedUser, PollOrderRequest pollOrderRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Id == id)
                .Include(c => c.Polls).FirstOrDefaultAsync();
            if (concept == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Concept");
            }
            var pollOne = concept.Polls.Where(p => p.Id == pollOrderRequest.PollOneId).FirstOrDefault();
            if (pollOne != null)
            {
                pollOne.Order = pollOrderRequest.PollOneOrder;
                await _uow.PollRepository.UpdateAsync(pollOne, pollOne.Id);
            }

            var pollTwo = concept.Polls.Where(p => p.Id == pollOrderRequest.PollTwoId).FirstOrDefault();
            if (pollTwo != null)
            {
                pollTwo.Order = pollOrderRequest.PollTwoOrder;
                await _uow.PollRepository.UpdateAsync(pollTwo, pollTwo.Id);
            }
            await _uow.CommitAsync();
        }

        public async Task ChangeConceptTranslationAsync(int loggedUser, ConceptTranslationRequest concetpTranslationRequest, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var concept = await _uow.ConceptRepository.GetAll().Where(c => c.Id == id)
                .FirstOrDefaultAsync();
            if (concept == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Concept");
            }

            switch (concetpTranslationRequest.Lang)
            {
                case "en":
                    concept.TitleEN = concetpTranslationRequest.Title;
                    concept.DescriptionEN = concetpTranslationRequest.Description;
                    concept.InstructionsEN = concetpTranslationRequest.Instructions;
                    break;

                case "it":
                    concept.TitleIT = concetpTranslationRequest.Title;
                    concept.DescriptionIT = concetpTranslationRequest.Description;
                    concept.InstructionsIT = concetpTranslationRequest.Instructions;
                    break;

                default:
                    concept.Title = concetpTranslationRequest.Title;
                    concept.Description = concetpTranslationRequest.Description;
                    concept.Instructions = concetpTranslationRequest.Instructions;
                    break;
            }

            _uow.ConceptRepository.Update(concept);
            await _uow.CommitAsync();
        }

        public async Task DeleteConceptAsync(int loggedUser, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var concept = await _uow.ConceptRepository.GetAsync(id);
            if (concept == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Concept");
            }

            if (!string.IsNullOrWhiteSpace(concept.Image))
            {
                await _fileService.DeleteFileAsync(concept.Image);
            }

            _uow.ConceptRepository.Delete(concept);
            await _uow.CommitAsync();
        }

        public async Task<Concept> EditConceptAsync(int loggedUser, UpdateConceptRequest concept, int id)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var conceptDb = await _uow.ConceptRepository.GetAsync(id);
            if (conceptDb == null)
            {
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Concept");
            }

            // validate concept name
            var existConceptName = await _uow.ConceptRepository.FindByAsync(p => p.Title.ToLower() == concept.Title.ToLower() && p.Id != id);
            if (existConceptName.Count > 0)
            {
                throw new InvalidDataException(ExceptionConstants.INVALID_DATA, "Concept name");
            }

            conceptDb.Title = concept.Title;
            conceptDb.Description = string.IsNullOrWhiteSpace(concept.Description) || concept.Description == "null" ? "" : concept.Description;
            conceptDb.ModifiedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(concept.RemovedImage) && concept.RemovedImage != "null")
            {
                await _fileService.DeleteFileAsync(conceptDb.Image);
                conceptDb.Image = "";
            }
            if (concept.Image != null)
            {
                string guid = Guid.NewGuid().ToString();
                await _fileService.UploadFileAsync(concept.Image, guid);
                conceptDb.Image = guid;
            }
            await _uow.ConceptRepository.UpdateAsync(conceptDb, id);
            await _uow.CommitAsync();

            return conceptDb;
        }

        public async Task<IEnumerable<Concept>> GetConceptsAdminAsync(int loggedUser)
        {
            // validate admin user
            var user = await _uow.UserRepository.FindByAsync(u => u.Id == loggedUser && u.Role == RoleEnum.ADMIN);
            if (user.Count == 0)
            {
                throw new NotAllowedException(ExceptionConstants.NOT_ALLOWED);
            }
            var result = await _uow.ConceptRepository.GetAll().Include(c => c.Polls).ToListAsync();
            return result;
        }

        public async Task<IEnumerable<Concept>> GetConceptsAsync()
        {
            var result = await _uow.ConceptRepository.GetAll().Include(c => c.Polls).ToListAsync();
            return result;
        }
    }
}