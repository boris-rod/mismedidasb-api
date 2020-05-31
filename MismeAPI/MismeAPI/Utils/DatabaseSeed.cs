using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MismeAPI.Utils
{
    public static class DatabaseSeed
    {
        public static async Task SeedDatabaseAsync(IServiceProvider serviceProvider)
        {
            var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var _categoryRewardService = serviceProvider.GetRequiredService<IRewardCategoryService>();
            var _soloQuestionService = serviceProvider.GetRequiredService<ISoloQuestionService>();

            await CreateAdminUserAsync(_uow);

            await _categoryRewardService.InitRewardCategoriesAsync();
            await _soloQuestionService.SeedSoloQuestionsAsync();

            //try
            //{
            //    ImportDishesAsync(services).Wait();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //RemoveDishesAsync(services).Wait();
        }

        private static async Task CreateAdminUserAsync(IUnitOfWork uow)
        {
            var admin = await uow.UserRepository.FindBy(u => u.Email == "admin@mismedidas.com").FirstOrDefaultAsync();

            if (admin == null)
            {
                using (var hashAlgorithm = new SHA256CryptoServiceProvider())
                {
                    var byteValue = Encoding.UTF8.GetBytes("P@ssw0rd");
                    var byteHash = hashAlgorithm.ComputeHash(byteValue);

                    admin = new User()
                    {
                        FullName = "Mismedidas Admin",
                        Email = "admin@mismedidas.com",
                        Password = Convert.ToBase64String(byteHash),
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow,
                        Role = RoleEnum.ADMIN,
                        Status = StatusEnum.ACTIVE
                    };
                    await uow.UserRepository.AddAsync(admin);
                    await uow.CommitAsync();
                }
            }
        }

        //private async Task ImportDishesAsync(IServiceProvider serviceProvider)
        //{
        //    var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        //    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //    using (var package = new ExcelPackage(new FileInfo("TablaAlimTransv2.xlsx")))
        //    {
        //        var sheetCount = package.Workbook.Worksheets.Count;
        //        var firstSheet = package.Workbook.Worksheets["Hoja1"];
        //        for (int i = 2; i <= 691; i++)
        //        //for (int i = 2; i <= 4; i++)
        //        {
        //            var dishName = firstSheet.Cells[i, 1].Text.Trim();
        //            var category = firstSheet.Cells[i, 2].Text.Trim();
        //            var calories = double.Parse(firstSheet.Cells[i, 8].Text.Trim());
        //            var carbohidrates = double.Parse(firstSheet.Cells[i, 9].Text.Trim());
        //            var proteins = double.Parse(firstSheet.Cells[i, 10].Text.Trim());
        //            var fat = double.Parse(firstSheet.Cells[i, 11].Text.Trim());
        //            var fiber = double.Parse(firstSheet.Cells[i, 12].Text.Trim());

        // var categoryBd = await _uow.TagRepository.GetAll().Where(t => t.Name ==
        // category).FirstOrDefaultAsync(); if (categoryBd == null) { categoryBd = new Tag();
        // categoryBd.Name = category; await _uow.TagRepository.AddAsync(categoryBd); await
        // _uow.CommitAsync(); } var tags = new List<DishTag>(); var dishTag = new DishTag();
        // dishTag.TagId = categoryBd.Id; dishTag.TaggedAt = DateTime.UtcNow; tags.Add(dishTag);

        //            var dish = new Dish();
        //            dish.Name = dishName;
        //            dish.Calories = calories;
        //            dish.Carbohydrates = carbohidrates;
        //            dish.Fat = fat;
        //            dish.Fiber = fiber;
        //            dish.Proteins = proteins;
        //            dish.DishTags = tags;
        //            await _uow.DishRepository.AddAsync(dish);
        //        }
        //        await _uow.CommitAsync();
        //    }
        //}

        //private async Task RemoveDishesAsync(IServiceProvider serviceProvider)
        //{
        //    var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
        //    var fileServ = serviceProvider.GetRequiredService<IFileService>();

        // var dishes = _uow.DishRepository.GetAll(); foreach (var dish in dishes) { if
        // (!string.IsNullOrWhiteSpace(dish.Image)) { await fileServ.DeleteFileAsync(dish.Image); }
        // _uow.DishRepository.Delete(dish); }

        //    var tags = _uow.TagRepository.GetAll();
        //    foreach (var tag in tags)
        //    {
        //        _uow.TagRepository.Delete(tag);
        //    }
        //    await _uow.CommitAsync();
        //}
    }
}
