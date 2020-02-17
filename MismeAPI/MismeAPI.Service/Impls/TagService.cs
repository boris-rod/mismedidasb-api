using Microsoft.EntityFrameworkCore;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _uow;

        public TagService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync()
        {
            var results = await _uow.TagRepository.GetAll().ToListAsync();
            return results;
        }
    }
}