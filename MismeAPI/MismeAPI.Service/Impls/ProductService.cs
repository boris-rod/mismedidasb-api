using MismeAPI.Common;
using MismeAPI.Common.Exceptions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Services.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MismeAPI.Services.Impls
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _uow;

        public ProductService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<PaginatedList<Product>> GetProductsAsync(int pag, int perPag, string sortOrder, string search)
        {
            var result = _uow.ProductRepository.GetAll().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                result = result.Where(i => i.Description.ToLower().Contains(search.ToLower()) || i.Name.ToString().Contains(search));
            }

            // define sort order
            if (!string.IsNullOrWhiteSpace(sortOrder))
            {
                // sort order section
                switch (sortOrder)
                {
                    case "price_desc":
                        result = result.OrderByDescending(i => i.Price);
                        break;

                    case "price_asc":
                        result = result.OrderBy(i => i.Price);
                        break;

                    case "name_desc":
                        result = result.OrderByDescending(i => i.Name);
                        break;

                    case "name_asc":
                        result = result.OrderBy(i => i.Name);
                        break;

                    default:
                        break;
                }
            }

            return await PaginatedList<Product>.CreateAsync(result, pag, perPag);
        }

        public async Task<Product> GetProductAsync(int id)
        {
            var product = await _uow.ProductRepository.GetAsync(id);
            if (product == null)
                throw new NotFoundException(ExceptionConstants.NOT_FOUND, "Product");

            return product;
        }

        public async Task SeedProductsAsync()
        {
            var now = DateTime.UtcNow;
            var count = await _uow.ProductRepository.CountAsync();

            if (count > 0)
                return;

            var product0 = new Product
            {
                Name = "Paquete de 500 monedas",
                Description = "Paquete de monedas para pagar servicios en PlaniFive",
                Price = 0.99m,
                Value = 500,
                CreatedAt = now,
                ModifiedAt = now,
                Type = ProductEnum.COIN_OFFER
            };
            await _uow.ProductRepository.AddAsync(product0);

            var product1 = new Product
            {
                Name = "Paquete de 2000 monedas",
                Description = "Paquete de monedas para pagar servicios en PlaniFive",
                Price = 2.99m,
                Value = 2000,
                CreatedAt = now,
                ModifiedAt = now,
                Type = ProductEnum.COIN_OFFER
            };
            await _uow.ProductRepository.AddAsync(product1);

            var product2 = new Product
            {
                Name = "Paquete de 3500 monedas",
                Description = "Paquete de monedas para pagar servicios en PlaniFive",
                Price = 4.99m,
                Value = 3500,
                CreatedAt = now,
                ModifiedAt = now,
                Type = ProductEnum.COIN_OFFER
            };
            await _uow.ProductRepository.AddAsync(product2);

            await _uow.CommitAsync();
        }
    }
}
