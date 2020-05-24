using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IFileService
    {
        Task UploadFileAsync(IFormFile file, string guid);

        Task DeleteFileAsync(string guid);

        Task CopyFileAsync(string sourceKey, string destinationKey);
    }
}