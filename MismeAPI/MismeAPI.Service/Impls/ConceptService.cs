using Microsoft.EntityFrameworkCore;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ConceptService : IConceptService
    {
        private readonly IUnitOfWork _uow;

        public ConceptService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<IEnumerable<Concept>> GetConceptsAsync()
        {
            var result = await _uow.ConceptRepository.GetAll().Include(c => c.Polls).ToListAsync();
            return result;
        }
    }
}