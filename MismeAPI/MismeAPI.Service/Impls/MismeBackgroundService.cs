using Microsoft.EntityFrameworkCore;
using MismeAPI.Data.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class MismeBackgroundService : IMismeBackgroundService
    {
        private readonly IUnitOfWork _uow;

        public MismeBackgroundService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task CleanExpiredTokensAsync()
        {
            var expiredTokens = await _uow.UserTokenRepository.GetAll().Where(t => DateTime.UtcNow > t.RefreshTokenExpiresDateTime).ToListAsync();
            foreach (var t in expiredTokens)
            {
                _uow.UserTokenRepository.Delete(t);
            }
            await _uow.CommitAsync();
        }
    }
}