using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MismeAPI.Data.Entities;
using MismeAPI.Data.Entities.Enums;
using MismeAPI.Data.UoW;
using MismeAPI.Service;
using MismeAPI.Services;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
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
            var _subscriptionService = serviceProvider.GetRequiredService<ISubscriptionService>();
            var _productService = serviceProvider.GetRequiredService<IProductService>();

            await CreateAdminUserAsync(_uow);

            await _categoryRewardService.InitRewardCategoriesAsync();
            await _soloQuestionService.SeedSoloQuestionsAsync();
            await _subscriptionService.SeedSubscriptionAsync();
            await _productService.SeedProductsAsync();
            //try
            //{
            //    //ImportDishesAsync(_uow, serviceProvider).Wait();
            //    //UpdateDishesAsync(_uow, serviceProvider).Wait();
            //    //RemoveDishesAsync(serviceProvider).Wait();
            //    //UploadHandCode(_uow, serviceProvider).Wait();
            //    //ImportHandCodeConversionValues(_uow, serviceProvider).Wait();
            //    //ChangeColumns(_uow, serviceProvider).Wait();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
        }

        private static async Task ChangeColumns(IUnitOfWork uow, IServiceProvider serviceProvider)
        {
            var dishes = uow.DishRepository.GetAll();
            foreach (var item in dishes)
            {
                var tempProteinsChanged = item.Carbohydrates;
                var tempCarboChanged = item.Proteins;
                item.Carbohydrates = tempCarboChanged;
                item.Proteins = tempProteinsChanged;
                await uow.DishRepository.UpdateAsync(item, item.Id);
            }

            await uow.CommitAsync();
        }

        private static async Task ImportHandCodeConversionValues(IUnitOfWork _uow, IServiceProvider serviceProvider)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/FACTORES C.xlsx")))
            {
                var sheetCount = package.Workbook.Worksheets.Count;
                var firstSheet = package.Workbook.Worksheets["Femenina"];
                for (int i = 2; i <= 54; i++)
                {
                    var height = int.Parse(firstSheet.Cells[i, 2].Text.Trim());
                    var conversion = double.Parse(firstSheet.Cells[i, 3].Text.Trim());
                    var conversion3 = double.Parse(firstSheet.Cells[i, 4].Text.Trim());
                    var conversion10 = double.Parse(firstSheet.Cells[i, 5].Text.Trim());
                    var conversion11 = double.Parse(firstSheet.Cells[i, 6].Text.Trim());
                    var conversion6 = double.Parse(firstSheet.Cells[i, 7].Text.Trim());

                    var conversionEntry = new HandConversionFactor();
                    conversionEntry.Height = height;
                    conversionEntry.Gender = GenderEnum.FEMALE;
                    conversionEntry.ConversionFactor = conversion;
                    conversionEntry.ConversionFactor3Code = conversion3;
                    conversionEntry.ConversionFactor6Code = conversion6;
                    conversionEntry.ConversionFactor10Code = conversion10;
                    conversionEntry.ConversionFactor11Code = conversion11;

                    await _uow.HandConversionFactorRepository.AddAsync(conversionEntry);
                }

                var secondSheet = package.Workbook.Worksheets["Masculino"];
                for (int i = 2; i <= 47; i++)
                {
                    var height = int.Parse(secondSheet.Cells[i, 2].Text.Trim());
                    var conversion = double.Parse(secondSheet.Cells[i, 3].Text.Trim());
                    var conversion3 = double.Parse(secondSheet.Cells[i, 4].Text.Trim());
                    var conversion10 = double.Parse(secondSheet.Cells[i, 5].Text.Trim());
                    var conversion11 = double.Parse(secondSheet.Cells[i, 6].Text.Trim());
                    var conversion6 = double.Parse(secondSheet.Cells[i, 7].Text.Trim());

                    var conversionEntry = new HandConversionFactor();
                    conversionEntry.Height = height;
                    conversionEntry.Gender = GenderEnum.MALE;
                    conversionEntry.ConversionFactor = conversion;
                    conversionEntry.ConversionFactor3Code = conversion3;
                    conversionEntry.ConversionFactor6Code = conversion6;
                    conversionEntry.ConversionFactor10Code = conversion10;
                    conversionEntry.ConversionFactor11Code = conversion11;

                    await _uow.HandConversionFactorRepository.AddAsync(conversionEntry);
                }
                await _uow.CommitAsync();
            }
        }

        private static async Task UploadHandCode(IUnitOfWork _uow, IServiceProvider serviceProvider)
        {
            var dishes = await _uow.DishRepository.GetAll().ToListAsync();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/dish_202009221630.xlsx")))
            {
                var sheetCount = package.Workbook.Worksheets.Count;
                var firstSheet = package.Workbook.Worksheets["these"];
                for (int i = 2; i <= 3365; i++)
                {
                    var id = int.Parse(firstSheet.Cells[i, 2].Text.Trim());
                    var code = int.Parse(firstSheet.Cells[i, 4].Text.Trim());
                    var dis = dishes.Where(d => d.Id == id).FirstOrDefault();
                    if (dis != null && dis.HandCode != code)
                    {
                        dis.HandCode = code;
                        _uow.DishRepository.Update(dis);
                    }
                }
                await _uow.CommitAsync();
            }
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

        private static async Task UpdateDishesAsync(IUnitOfWork _uow, IServiceProvider serviceProvider)
        {
            var fileService = serviceProvider.GetRequiredService<IAmazonS3Service>();

            var handsCodes = new Dictionary<int, string>();
            handsCodes.Add(0, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/0.png");
            handsCodes.Add(1, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/1.png");
            handsCodes.Add(2, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/2.png");
            handsCodes.Add(3, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/3.png");
            handsCodes.Add(4, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/4.png");
            handsCodes.Add(5, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/5.png");
            handsCodes.Add(6, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/6.png");
            handsCodes.Add(7, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/7.png");
            handsCodes.Add(8, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/8.png");
            handsCodes.Add(9, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/9.png");
            handsCodes.Add(10, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/10.png");
            handsCodes.Add(11, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/11.png");
            handsCodes.Add(12, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/12.png");
            handsCodes.Add(13, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/13.png");
            handsCodes.Add(14, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/14.jpg");
            handsCodes.Add(15, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/15.png");
            handsCodes.Add(16, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/16.png");
            handsCodes.Add(17, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/17.png");
            handsCodes.Add(18, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/18.jpg");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/Envido  nuevos solo y para eliminar.xlsx")))
            using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/Modificada final.xlsx")))
            {
                var sheetCount = package.Workbook.Worksheets.Count;
                var firstSheet = package.Workbook.Worksheets["Hoja1"];
                //for (int i = 2; i <= 3829; i++)
                for (int i = 3501; i <= 3829; i++)
                {
                    var id = int.Parse(firstSheet.Cells[i, 1].Text.Trim());
                    var dishDB = await _uow.DishRepository.GetAsync(id);
                    if (dishDB != null)
                    {
                        //var code = firstSheet.Cells[i, 2].Text.Trim();
                        var dishName = firstSheet.Cells[i, 3].Text.Trim();
                        var handsCode = int.Parse(firstSheet.Cells[i, 4].Text.Trim());

                        var netWeight = double.Parse(firstSheet.Cells[i, 5].Text.Trim());
                        var volume = double.Parse(firstSheet.Cells[i, 6].Text.Trim());
                        //var dishClasifi = firstSheet.Cells[i, 7].Text.Trim();
                        //var category = firstSheet.Cells[i, 8].Text.Trim();
                        var calories = double.Parse(firstSheet.Cells[i, 9].Text.Trim());
                        var proteins = double.Parse(firstSheet.Cells[i, 10].Text.Trim());
                        var carbohidrates = double.Parse(firstSheet.Cells[i, 11].Text.Trim());
                        var fiber = double.Parse(firstSheet.Cells[i, 12].Text.Trim());
                        var fat = double.Parse(firstSheet.Cells[i, 13].Text.Trim());
                        var staurated = double.Parse(firstSheet.Cells[i, 14].Text.Trim());
                        var monoUnsaturated = double.Parse(firstSheet.Cells[i, 15].Text.Trim());
                        var polyUnsaturated = double.Parse(firstSheet.Cells[i, 16].Text.Trim());
                        var cholesterol = double.Parse(firstSheet.Cells[i, 17].Text.Trim());
                        var vitaminA = double.Parse(firstSheet.Cells[i, 18].Text.Trim());
                        var vitaminB1Thiamin = double.Parse(firstSheet.Cells[i, 19].Text.Trim());
                        var vitaminB2Riboflavin = double.Parse(firstSheet.Cells[i, 20].Text.Trim());
                        var vitaminB3Niacin = double.Parse(firstSheet.Cells[i, 21].Text.Trim());
                        var vitaminB6 = double.Parse(firstSheet.Cells[i, 22].Text.Trim());
                        var vitaminB9Folate = double.Parse(firstSheet.Cells[i, 23].Text.Trim());
                        var vitaminB12 = double.Parse(firstSheet.Cells[i, 24].Text.Trim());
                        var vitaminC = double.Parse(firstSheet.Cells[i, 25].Text.Trim());
                        var vitaminD = double.Parse(firstSheet.Cells[i, 26].Text.Trim());
                        var vitaminE = double.Parse(firstSheet.Cells[i, 27].Text.Trim());
                        var vitaminK = double.Parse(firstSheet.Cells[i, 28].Text.Trim());
                        var calcium = double.Parse(firstSheet.Cells[i, 29].Text.Trim());
                        var phosporus = double.Parse(firstSheet.Cells[i, 30].Text.Trim());
                        var iron = double.Parse(firstSheet.Cells[i, 31].Text.Trim());
                        var zinc = double.Parse(firstSheet.Cells[i, 32].Text.Trim());
                        var potsassium = double.Parse(firstSheet.Cells[i, 33].Text.Trim());
                        var sodium = double.Parse(firstSheet.Cells[i, 34].Text.Trim());
                        var alcohol = double.Parse(firstSheet.Cells[i, 35].Text.Trim());

                        if (dishDB.HandCode != handsCode)
                        {
                            //upload images
                            string guid = Guid.NewGuid().ToString();

                            using (FileStream f = new FileStream(handsCodes[handsCode], FileMode.Open, FileAccess.Read))
                            {
                                MemoryStream ms = new MemoryStream();
                                f.CopyTo(ms);
                                await fileService.PutObjectAsync(guid, ms);
                            }
                            dishDB.Image = guid;
                            dishDB.HandCode = handsCode;
                        }

                        dishDB.Name = dishName;
                        if (netWeight >= 0)
                        {
                            dishDB.NetWeight = netWeight;
                        }
                        if (volume >= 0)
                        {
                            dishDB.Volume = volume;
                        }
                        if (calories >= 0)
                        {
                            dishDB.Calories = calories;
                        }
                        if (proteins >= 0)
                        {
                            dishDB.Proteins = proteins;
                        }
                        if (carbohidrates >= 0)
                        {
                            dishDB.Carbohydrates = carbohidrates;
                        }
                        if (fiber >= 0)
                        {
                            dishDB.Fiber = fiber;
                        }
                        if (fat >= 0)
                        {
                            dishDB.Fat = fat;
                        }
                        if (staurated >= 0)
                        {
                            dishDB.SaturatedFat = staurated;
                        }
                        if (monoUnsaturated >= 0)
                        {
                            dishDB.MonoUnsaturatedFat = monoUnsaturated;
                        }
                        if (polyUnsaturated >= 0)
                        {
                            dishDB.PolyUnsaturatedFat = polyUnsaturated;
                        }
                        if (cholesterol >= 0)
                        {
                            dishDB.Cholesterol = cholesterol;
                        }
                        if (vitaminA >= 0)
                        {
                            dishDB.VitaminA = vitaminA;
                        }
                        if (vitaminB1Thiamin >= 0)
                        {
                            dishDB.VitaminB1Thiamin = vitaminB1Thiamin;
                        }
                        if (vitaminB2Riboflavin >= 0)
                        {
                            dishDB.VitaminB2Riboflavin = vitaminB2Riboflavin;
                        }
                        if (vitaminB3Niacin >= 0)
                        {
                            dishDB.VitaminB3Niacin = vitaminB3Niacin;
                        }

                        if (vitaminB6 >= 0)
                        {
                            dishDB.VitaminB6 = vitaminB6;
                        }
                        if (vitaminB9Folate >= 0)
                        {
                            dishDB.VitaminB9Folate = vitaminB9Folate;
                        }
                        if (vitaminB12 >= 0)
                        {
                            dishDB.VitaminB12 = vitaminB12;
                        }
                        if (vitaminC >= 0)
                        {
                            dishDB.VitaminC = vitaminC;
                        }
                        if (vitaminD >= 0)
                        {
                            dishDB.VitaminD = vitaminD;
                        }
                        if (vitaminE >= 0)
                        {
                            dishDB.VitaminE = vitaminE;
                        }
                        if (vitaminK >= 0)
                        {
                            dishDB.VitaminK = vitaminK;
                        }

                        if (calcium >= 0)
                        {
                            dishDB.Calcium = calcium;
                        }
                        if (phosporus >= 0)
                        {
                            dishDB.Phosphorus = phosporus;
                        }
                        if (iron >= 0)
                        {
                            dishDB.Iron = iron;
                        }
                        if (zinc >= 0)
                        {
                            dishDB.Zinc = zinc;
                        }
                        if (potsassium >= 0)
                        {
                            dishDB.Potassium = potsassium;
                        }
                        if (sodium >= 0)
                        {
                            dishDB.Sodium = sodium;
                        }
                        if (alcohol >= 0)
                        {
                            dishDB.Alcohol = alcohol;
                        }
                        dishDB.ModifiedAt = DateTime.UtcNow;
                        _uow.DishRepository.Update(dishDB);
                    }
                }

                await _uow.CommitAsync();
            }
        }

        private static async Task ImportDishesAsync(IUnitOfWork _uow, IServiceProvider serviceProvider)
        {
            var fileService = serviceProvider.GetRequiredService<IAmazonS3Service>();

            var handsCodes = new Dictionary<int, string>();
            handsCodes.Add(0, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/0.png");
            handsCodes.Add(1, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/1.png");
            handsCodes.Add(2, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/2.png");
            handsCodes.Add(3, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/3.png");
            handsCodes.Add(4, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/4.png");
            handsCodes.Add(5, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/5.png");
            handsCodes.Add(6, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/6.png");
            handsCodes.Add(7, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/7.png");
            handsCodes.Add(8, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/8.png");
            handsCodes.Add(9, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/9.png");
            handsCodes.Add(10, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/10.png");
            handsCodes.Add(11, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/11.png");
            handsCodes.Add(12, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/12.png");
            handsCodes.Add(13, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/13.png");
            handsCodes.Add(14, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/14.jpg");
            handsCodes.Add(15, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/15.png");
            handsCodes.Add(16, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/16.png");
            handsCodes.Add(17, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/17.png");
            handsCodes.Add(18, "D:/Projects/Mismes/BD Alimentos Saira/Alimentos 100x100/18.jpg");

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/Envido  nuevos solo y para eliminar.xlsx")))
            using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/Poner en produccion/Enviar Yoandy10102020.xlsx")))
            {
                var sheetCount = package.Workbook.Worksheets.Count;
                var firstSheet = package.Workbook.Worksheets["Incluido 2da etapa"];
                for (int i = 701; i <= 781; i++)
                {
                    var code = firstSheet.Cells[i, 2].Text.Trim();
                    var dishName = firstSheet.Cells[i, 3].Text.Trim();
                    var handsCode = int.Parse(firstSheet.Cells[i, 4].Text.Trim());

                    var netWeight = double.Parse(firstSheet.Cells[i, 5].Text.Trim());
                    var volume = double.Parse(firstSheet.Cells[i, 6].Text.Trim());
                    var dishClasifi = firstSheet.Cells[i, 7].Text.Trim();
                    var category = firstSheet.Cells[i, 8].Text.Trim();
                    var calories = double.Parse(firstSheet.Cells[i, 9].Text.Trim());
                    var proteins = double.Parse(firstSheet.Cells[i, 10].Text.Trim());
                    var carbohidrates = double.Parse(firstSheet.Cells[i, 11].Text.Trim());
                    var fiber = double.Parse(firstSheet.Cells[i, 12].Text.Trim());
                    var fat = double.Parse(firstSheet.Cells[i, 13].Text.Trim());
                    var staurated = double.Parse(firstSheet.Cells[i, 14].Text.Trim());
                    var monoUnsaturated = double.Parse(firstSheet.Cells[i, 15].Text.Trim());
                    var polyUnsaturated = double.Parse(firstSheet.Cells[i, 16].Text.Trim());
                    var cholesterol = double.Parse(firstSheet.Cells[i, 17].Text.Trim());
                    var vitaminA = double.Parse(firstSheet.Cells[i, 18].Text.Trim());
                    var vitaminB1Thiamin = double.Parse(firstSheet.Cells[i, 19].Text.Trim());
                    var vitaminB2Riboflavin = double.Parse(firstSheet.Cells[i, 20].Text.Trim());
                    var vitaminB3Niacin = double.Parse(firstSheet.Cells[i, 21].Text.Trim());
                    var vitaminB6 = double.Parse(firstSheet.Cells[i, 22].Text.Trim());
                    var vitaminB9Folate = double.Parse(firstSheet.Cells[i, 23].Text.Trim());
                    var vitaminB12 = double.Parse(firstSheet.Cells[i, 24].Text.Trim());
                    var vitaminC = double.Parse(firstSheet.Cells[i, 25].Text.Trim());
                    var vitaminD = double.Parse(firstSheet.Cells[i, 26].Text.Trim());
                    var vitaminE = double.Parse(firstSheet.Cells[i, 27].Text.Trim());
                    var vitaminK = double.Parse(firstSheet.Cells[i, 28].Text.Trim());
                    var calcium = double.Parse(firstSheet.Cells[i, 29].Text.Trim());
                    var phosporus = double.Parse(firstSheet.Cells[i, 30].Text.Trim());
                    var iron = double.Parse(firstSheet.Cells[i, 31].Text.Trim());
                    var zinc = double.Parse(firstSheet.Cells[i, 32].Text.Trim());
                    var potsassium = double.Parse(firstSheet.Cells[i, 33].Text.Trim());
                    var sodium = double.Parse(firstSheet.Cells[i, 34].Text.Trim());

                    var tagsDict = new Dictionary<string, int>();
                    tagsDict.Add("Bebidas", 12);
                    tagsDict.Add("Frutas", 13);
                    tagsDict.Add("Grasas/aderezos/salsas", 14);
                    tagsDict.Add("Lácteos y derivados", 15);
                    tagsDict.Add("Platos combinados", 16);
                    tagsDict.Add("Postre/dulces/complementos", 17);
                    tagsDict.Add("Proteína animal", 18);
                    tagsDict.Add("Proteína vegetal", 19);
                    tagsDict.Add("Sopas/cereales/tubérculos", 20);
                    tagsDict.Add("Vegetales y verduras", 21);

                    var tags = new List<DishTag>();
                    var dishTag = new DishTag();
                    dishTag.TagId = tagsDict[category];
                    dishTag.TaggedAt = DateTime.UtcNow; tags.Add(dishTag);

                    //upload images
                    string guid = Guid.NewGuid().ToString();

                    using (FileStream f = new FileStream(handsCodes[handsCode], FileMode.Open, FileAccess.Read))
                    {
                        MemoryStream ms = new MemoryStream();
                        f.CopyTo(ms);
                        await fileService.PutObjectAsync(guid, ms);
                    }
                    var dish = new Dish();
                    dish.Image = guid;
                    dish.Code = code;
                    dish.Name = dishName;
                    dish.HandCode = handsCode;
                    if (netWeight >= 0)
                    {
                        dish.NetWeight = netWeight;
                    }
                    if (volume >= 0)
                    {
                        dish.Volume = volume;
                    }
                    if (calories >= 0)
                    {
                        dish.Calories = calories;
                    }
                    if (proteins >= 0)
                    {
                        dish.Proteins = proteins;
                    }
                    if (carbohidrates >= 0)
                    {
                        dish.Carbohydrates = carbohidrates;
                    }
                    if (fiber >= 0)
                    {
                        dish.Fiber = fiber;
                    }
                    if (fat >= 0)
                    {
                        dish.Fat = fat;
                    }
                    if (staurated >= 0)
                    {
                        dish.SaturatedFat = staurated;
                    }
                    if (monoUnsaturated >= 0)
                    {
                        dish.MonoUnsaturatedFat = monoUnsaturated;
                    }
                    if (polyUnsaturated >= 0)
                    {
                        dish.PolyUnsaturatedFat = polyUnsaturated;
                    }
                    if (cholesterol >= 0)
                    {
                        dish.Cholesterol = cholesterol;
                    }
                    if (vitaminA >= 0)
                    {
                        dish.VitaminA = vitaminA;
                    }
                    if (vitaminB1Thiamin >= 0)
                    {
                        dish.VitaminB1Thiamin = vitaminB1Thiamin;
                    }
                    if (vitaminB2Riboflavin >= 0)
                    {
                        dish.VitaminB2Riboflavin = vitaminB2Riboflavin;
                    }
                    if (vitaminB3Niacin >= 0)
                    {
                        dish.VitaminB3Niacin = vitaminB3Niacin;
                    }

                    if (vitaminB6 >= 0)
                    {
                        dish.VitaminB6 = vitaminB6;
                    }
                    if (vitaminB9Folate >= 0)
                    {
                        dish.VitaminB9Folate = vitaminB9Folate;
                    }
                    if (vitaminB12 >= 0)
                    {
                        dish.VitaminB12 = vitaminB12;
                    }
                    if (vitaminC >= 0)
                    {
                        dish.VitaminC = vitaminC;
                    }
                    if (vitaminD >= 0)
                    {
                        dish.VitaminD = vitaminD;
                    }
                    if (vitaminE >= 0)
                    {
                        dish.VitaminE = vitaminE;
                    }
                    if (vitaminK >= 0)
                    {
                        dish.VitaminK = vitaminK;
                    }

                    if (calcium >= 0)
                    {
                        dish.Calcium = calcium;
                    }
                    if (phosporus >= 0)
                    {
                        dish.Phosphorus = phosporus;
                    }
                    if (iron >= 0)
                    {
                        dish.Iron = iron;
                    }
                    if (zinc >= 0)
                    {
                        dish.Zinc = zinc;
                    }
                    if (potsassium >= 0)
                    {
                        dish.Potassium = potsassium;
                    }
                    if (sodium >= 0)
                    {
                        dish.Sodium = sodium;
                    }

                    switch (dishClasifi)
                    {
                        case "Calórico":
                            dish.IsCaloric = true;
                            break;

                        case "Protéico":
                            dish.IsProteic = true;
                            break;

                        default:
                            dish.IsFruitAndVegetables = true;
                            break;
                    }

                    dish.DishTags = tags;
                    dish.CreatedAt = DateTime.UtcNow;
                    dish.ModifiedAt = DateTime.UtcNow;
                    await _uow.DishRepository.AddAsync(dish);
                }

                await _uow.CommitAsync();
            }
        }

        private static async Task RemoveDishesAsync(IServiceProvider serviceProvider)
        {
            var _uow = serviceProvider.GetRequiredService<IUnitOfWork>();
            var fileServ = serviceProvider.GetRequiredService<IFileService>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo("D:/Projects/Mismes/BD Alimentos Saira/Base Datos Yoandry para editar3.xlsx")))
            {
                var sheetCount = package.Workbook.Worksheets.Count;
                var firstSheet = package.Workbook.Worksheets["Delete"]; //1159
                for (int i = 1151; i <= 1159; i++)
                {
                    var id = int.Parse(firstSheet.Cells[i, 1].Text.Trim());
                    var dish = await _uow.DishRepository.GetAsync(id);
                    if (dish != null)
                    {
                        if (!string.IsNullOrWhiteSpace(dish.Image))
                        {
                            try
                            {
                                await fileServ.DeleteFileAsync(dish.Image);
                            }
                            catch (Exception) { }
                        }
                        _uow.DishRepository.Delete(dish);
                    }
                }
                await _uow.CommitAsync();
            }
        }
    }
}