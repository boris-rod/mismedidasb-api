using MismeAPI.Common.DTO.Request.Device;
using System.Threading.Tasks;

namespace MismeAPI.Service
{
    public interface IDeviceService
    {
        Task CreateOrUpdateDeviceAsync(int loggedUser, AddDeviceRequest device);
    }
}