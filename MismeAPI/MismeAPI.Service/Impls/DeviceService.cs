using Microsoft.EntityFrameworkCore;
using MismeAPI.Common.DTO.Request.Device;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using System;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _uow;

        public DeviceService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task CreateOrUpdateDeviceAsync(int loggedUser, AddDeviceRequest device)
        {
            var dev = await _uow.DeviceRepository.FindBy(d => d.DeviceId == device.DeviceId && d.UserId == loggedUser).FirstOrDefaultAsync();
            if (dev == null)
            {
                dev = new Device();
                dev.Token = device.Token;
                dev.DeviceId = device.DeviceId;
                dev.UserId = loggedUser;
                await _uow.DeviceRepository.AddAsync(dev);
            }
            else
            {
                dev.Token = device.Token;
                await _uow.DeviceRepository.UpdateAsync(dev, dev.Id);
            }
            await _uow.CommitAsync();
        }
    }
}