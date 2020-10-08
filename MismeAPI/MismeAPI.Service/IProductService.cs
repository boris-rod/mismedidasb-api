using Amazon.S3;
using Amazon.S3.Model;
using MismeAPI.Data.Entities;
using MismeAPI.Services.Utils;
using System.IO;
using System.Threading.Tasks;

namespace MismeAPI.Services
{
    public interface IProductService
    {
        Task<PaginatedList<Product>> GetProductsAsync(int pag, int perPag, string sortOrder, string search);

        Task<Product> GetProductAsync(int id);

        Task SeedProductsAsync();
    }
}
