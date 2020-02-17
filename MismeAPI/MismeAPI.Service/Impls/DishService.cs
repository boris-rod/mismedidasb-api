using MismeAPI.Data.UoW;
using System;

namespace MismeAPI.Service.Impls
{
    public class DishService : IDishService
    {
        private readonly IUnitOfWork _uow;

        public DishService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }
    }
}