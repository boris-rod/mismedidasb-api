using MismeAPI.Data.UoW;
using System;

namespace MismeAPI.Service.Impls
{
    public class PersonalDataService : IPersonalDataService
    {
        private readonly IUnitOfWork _uow;

        public PersonalDataService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }
    }
}