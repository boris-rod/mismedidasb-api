using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using MismeAPI.Common.DTO;
using MismeAPI.Common.Extensions;
using MismeAPI.Data.Entities;
using MismeAPI.Data.UoW;
using MismeAPI.Services;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IAccountService _accountService;
        private readonly IPollService _pollService;

        public ReportService(IUnitOfWork uow, IAccountService accountService, IPollService pollService)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
        }

        public Task GetFeedReportAsync()
        {
            throw new NotImplementedException();
        }

        public async Task GetNutritionalReport(int userId)
        {
            var result = await _accountService.GetUserProfileUseAsync(userId);

            //var image = GetImage();
            var cover = await GetCoverContent(result.user);
            //var charData = GetChartsContent();
            var contents = new List<MemoryStream>();
            contents.Add(new MemoryStream(cover));
            //contents.Add(new MemoryStream(charData));

            using (var mergedFileStream = new MemoryStream())
            {
                new MergePdfDocuments
                {
                    DocumentMetadata =
                     new DocumentMetadata
                     {
                         Author = "PlaniFive",
                         Application = "PlaniFive",
                         Keywords = "PlaniFive, Nutricion, Reporte",
                         Title = "Reporte Nutricion",
                         Subject = "Reporte Nutricion"
                     },
                    InputFileStreams = contents.ToArray(),
                    OutputFileStream = mergedFileStream,
                    AttachmentsBookmarkLabel = "Attachment(s) "
                }.PerformMerge();

                var mergedFileContentBytes = mergedFileStream.ToArray();

                File.WriteAllBytes("D:\\ReporteNutricion.pdf", mergedFileContentBytes);
            }
        }

        private void AddChartToPage(Document pdfDoc,
            List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish,
                                   float spacingBefore = 5,
                                   float spacingAfter = 5,
                                   float widthPercentage = 100)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var dict = new Dictionary<int, double>();
            dict.Add((int)DayOfWeek.Sunday, 0.0);
            dict.Add((int)DayOfWeek.Monday, 0.0);
            dict.Add((int)DayOfWeek.Tuesday, 0.0);
            dict.Add((int)DayOfWeek.Wednesday, 0.0);
            dict.Add((int)DayOfWeek.Thursday, 0.0);
            dict.Add((int)DayOfWeek.Friday, 0.0);
            dict.Add((int)DayOfWeek.Saturday, 0.0);

            foreach (var group in gr)
            {
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    temp += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }
                dict[(int)group.Key.DayOfWeek] = Math.Round(temp, 2);
                temp = 0;
            }

            var plt = new ScottPlot.Plot(500, 200);

            string[] labels = new string[7];
            double[] xs = new double[7];
            double[] ys = new double[7];

            foreach (var item in dict)
            {
                switch (item.Key)
                {
                    case 0:
                        labels[0] = "Domingo";
                        xs[0] = 0.0;
                        ys[0] = item.Value;
                        break;

                    case 1:
                        labels[1] = "Lunes";
                        xs[1] = 1.0;
                        ys[1] = item.Value;
                        break;

                    case 2:
                        labels[2] = "Martes";
                        xs[2] = 2.0;
                        ys[2] = item.Value;
                        break;

                    case 3:
                        labels[3] = "Miércoles";
                        xs[3] = 3.0;
                        ys[3] = item.Value;
                        break;

                    case 4:
                        labels[4] = "Jueves";
                        xs[4] = 4.0;
                        ys[4] = item.Value;
                        break;

                    case 5:
                        labels[5] = "Viernes";
                        xs[5] = 5.0;
                        ys[5] = item.Value;
                        break;

                    default:
                        labels[6] = "Sábado";
                        xs[6] = 6.0;
                        ys[6] = item.Value;
                        break;
                }
            }

            plt.PlotBar(xs, ys, showValues: true);
            plt.XTicks(labels);

            // improve the styling
            plt.Legend(enableLegend: true);
            plt.Title("Total kcal/día", enable: true);

            //var currentAssembly = typeof(ReportService).GetTypeInfo().Assembly;
            //var root = Path.GetDirectoryName(currentAssembly.Location);
            plt.SaveFig("kcalPerDay.png");

            var imageBytes = File.ReadAllBytes("kcalPerDay.png");
            var iTextSharpImage = PdfImageHelper.GetITextSharpImageFromByteArray(imageBytes);
            iTextSharpImage.Alignment = Element.ALIGN_CENTER;

            var tableImage = new PdfPTable(1);
            tableImage.HorizontalAlignment = Element.ALIGN_CENTER;
            tableImage.DefaultCell.Border = Rectangle.NO_BORDER;
            tableImage.WidthPercentage = 100;
            tableImage.SpacingAfter = 60;
            tableImage.SpacingBefore = 100;

            tableImage.AddCell(iTextSharpImage);

            /// kcal per day: STACKED
            double[] proteins = new double[7];
            double[] carbo = new double[7];
            double[] fats = new double[7];

            var dictProt = new Dictionary<int, double>();
            dictProt.Add((int)DayOfWeek.Sunday, 0.0);
            dictProt.Add((int)DayOfWeek.Monday, 0.0);
            dictProt.Add((int)DayOfWeek.Tuesday, 0.0);
            dictProt.Add((int)DayOfWeek.Wednesday, 0.0);
            dictProt.Add((int)DayOfWeek.Thursday, 0.0);
            dictProt.Add((int)DayOfWeek.Friday, 0.0);
            dictProt.Add((int)DayOfWeek.Saturday, 0.0);

            var dictCarbo = new Dictionary<int, double>();
            dictCarbo.Add((int)DayOfWeek.Sunday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Monday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Tuesday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Wednesday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Thursday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Friday, 0.0);
            dictCarbo.Add((int)DayOfWeek.Saturday, 0.0);

            var dictFats = new Dictionary<int, double>();
            dictFats.Add((int)DayOfWeek.Sunday, 0.0);
            dictFats.Add((int)DayOfWeek.Monday, 0.0);
            dictFats.Add((int)DayOfWeek.Tuesday, 0.0);
            dictFats.Add((int)DayOfWeek.Wednesday, 0.0);
            dictFats.Add((int)DayOfWeek.Thursday, 0.0);
            dictFats.Add((int)DayOfWeek.Friday, 0.0);
            dictFats.Add((int)DayOfWeek.Saturday, 0.0);

            foreach (var group in gr)
            {
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Proteins ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    temp += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty)) * e.Qty);
                }
                dictProt[(int)group.Key.DayOfWeek] = Math.Round(temp, 2);
                temp = 0;

                temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Carbohydrates ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    temp += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty)) * e.Qty);
                }
                dictCarbo[(int)group.Key.DayOfWeek] = Math.Round(temp, 2);
                temp = 0;

                temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Fat ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    temp += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty)) * e.Qty);
                }
                dictFats[(int)group.Key.DayOfWeek] = Math.Round(temp, 2);
                temp = 0;
            }

            foreach (var item in dictProt)
            {
                var tot = Math.Round(item.Value + dictCarbo[item.Key] + dictFats[item.Key], 2);
                switch (item.Key)
                {
                    case 0:
                        //labels[0] = "Domingo";
                        xs[0] = 0.0;
                        proteins[0] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[0] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[0] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    case 1:
                        //labels[1] = "Lunes";
                        xs[1] = 1.0;
                        proteins[1] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[1] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[1] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    case 2:
                        //labels[2] = "Martes";
                        xs[2] = 2.0;
                        proteins[2] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[2] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[2] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    case 3:
                        //labels[3] = "Miércoles";
                        xs[3] = 3.0;
                        proteins[3] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[3] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[3] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    case 4:
                        //labels[4] = "Jueves";
                        xs[4] = 4.0;
                        proteins[4] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[4] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[4] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    case 5:
                        //labels[5] = "Viernes";
                        xs[5] = 5.0;
                        proteins[5] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[5] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[5] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;

                    default:
                        //labels[6] = "Sábado";
                        xs[6] = 6.0;
                        proteins[6] = tot > 0 ? item.Value / tot : 0.0;
                        carbo[6] = tot > 0 ? dictCarbo[item.Key] / tot : 0.0;
                        fats[6] = tot > 0 ? dictFats[item.Key] / tot : 0.0;
                        break;
                }
            }

            var plt1 = new ScottPlot.Plot(500, 400);

            plt1.PlotBar(xs, proteins, label: "Proteínas", barWidth: .2, xOffset: -.3);
            plt1.PlotBar(xs, carbo, label: "Carbohidratos", barWidth: .2, xOffset: .0);
            plt1.PlotBar(xs, fats, label: "Grasas", barWidth: .2, xOffset: .3);

            plt1.XTicks(labels);
            plt1.Ticks(numericFormatStringY: "P1");

            // improve the styling
            //plt.Legend(enableLegend: true);
            plt1.Legend(location: ScottPlot.legendLocation.upperRight);
            plt1.Title("Aportes en % a las kcal/día", enable: true);

            //var currentAssembly = typeof(ReportService).GetTypeInfo().Assembly;
            //var root = Path.GetDirectoryName(currentAssembly.Location);
            plt1.SaveFig("kcalPerDayStack.png");

            var imageBytes1 = File.ReadAllBytes("kcalPerDayStack.png");
            var iTextSharpImage1 = PdfImageHelper.GetITextSharpImageFromByteArray(imageBytes1);
            iTextSharpImage1.Alignment = Element.ALIGN_CENTER;

            var tableImage1 = new PdfPTable(1);
            tableImage1.HorizontalAlignment = Element.ALIGN_CENTER;
            tableImage1.DefaultCell.Border = Rectangle.NO_BORDER;
            tableImage1.WidthPercentage = 100;

            tableImage1.AddCell(iTextSharpImage1);

            pdfDoc.Add(tableImage);
            pdfDoc.Add(tableImage1);

            File.Delete("kcalPerDay.png");
            File.Delete("kcalPerDayStack.png");
        }

        private List<MacroMicroValues> GetData(List<EatDish> eats, List<EatCompoundDish> compoundEats)
        {
            var list = new List<MacroMicroValues>();

            //PROTEINS
            var max = 0.0;
            var min = 1000000000.0;

            var total1 = eats.Sum(e => (e.Dish.Proteins ?? 0.0) * e.Qty);
            var total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty)) * e.Qty);

            // this is taking into account only the days planned. The other option is to divide by 7 fixed.
            var groups1 = eats.GroupBy(e => e.Eat.CreatedAt.Date);
            var groups2 = compoundEats.GroupBy(e => e.Eat.CreatedAt.Date);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Proteins ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            var avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            var avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.PROTEINS,
                TypeString = MacroMicroType.PROTEINS.GetDescription()
            });

            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //ALCOHOL
            total1 = eats.Sum(e => (e.Dish.Alcohol ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Alcohol ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Alcohol ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Alcohol ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.ALCOHOL,
                TypeString = MacroMicroType.ALCOHOL.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            // CALCIUM
            total1 = eats.Sum(e => (e.Dish.Calcium ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calcium ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calcium ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calcium ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.CALCIUM,
                TypeString = MacroMicroType.CALCIUM.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            // CARBOHYDRATES
            total1 = eats.Sum(e => (e.Dish.Carbohydrates ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Carbohydrates ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.CARBOHIDRATES,
                TypeString = MacroMicroType.CARBOHIDRATES.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            // CHOLESTEROL
            total1 = eats.Sum(e => (e.Dish.Cholesterol ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Cholesterol ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Cholesterol ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Cholesterol ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.CHOLESTEROL,
                TypeString = MacroMicroType.CHOLESTEROL.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            // FAT
            total1 = eats.Sum(e => (e.Dish.Fat ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Fat ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FAT,
                TypeString = MacroMicroType.FAT.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            // FIBER
            total1 = eats.Sum(e => (e.Dish.Fiber ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fiber ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Fiber ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fiber ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FIBER,
                TypeString = MacroMicroType.FIBER.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //IRON
            total1 = eats.Sum(e => (e.Dish.Iron ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Iron ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Iron ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Iron ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.IRON,
                TypeString = MacroMicroType.IRON.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //MONO UNSATURATED FAT
            total1 = eats.Sum(e => (e.Dish.MonoUnsaturatedFat ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.MonoUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.MonoUnsaturatedFat ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.MonoUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FAT_ACID_MONOINSATURATE,
                TypeString = MacroMicroType.FAT_ACID_MONOINSATURATE.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //PHOSPHORUS
            total1 = eats.Sum(e => (e.Dish.Phosphorus ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Phosphorus ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Phosphorus ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Phosphorus ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.PHOSPHORUS,
                TypeString = MacroMicroType.PHOSPHORUS.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //POLY INSATURATED FAT
            total1 = eats.Sum(e => (e.Dish.PolyUnsaturatedFat ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.PolyUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.PolyUnsaturatedFat ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.PolyUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FAT_ACID_POLYINSATURATE,
                TypeString = MacroMicroType.FAT_ACID_POLYINSATURATE.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //POTASSIUM
            total1 = eats.Sum(e => (e.Dish.Potassium ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Potassium ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Potassium ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Potassium ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.POTASSIUM,
                TypeString = MacroMicroType.POTASSIUM.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //SATURATED FAT
            total1 = eats.Sum(e => (e.Dish.SaturatedFat ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.SaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.SaturatedFat ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.SaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FAT_ACID_SATURATE,
                TypeString = MacroMicroType.FAT_ACID_SATURATE.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //SODIUM
            total1 = eats.Sum(e => (e.Dish.Sodium ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Sodium ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Sodium ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Sodium ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.SODIUM,
                TypeString = MacroMicroType.SODIUM.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN A
            total1 = eats.Sum(e => (e.Dish.VitaminA ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminA ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminA ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminA ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMINA,
                TypeString = MacroMicroType.VITAMINA.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B12
            total1 = eats.Sum(e => (e.Dish.VitaminB12 ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB12 ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB12 ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB12 ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_B12,
                TypeString = MacroMicroType.VITAMIN_B12.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B1 Thiamin
            total1 = eats.Sum(e => (e.Dish.VitaminB1Thiamin ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB1Thiamin ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB1Thiamin ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB1Thiamin ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.TIAMIN,
                TypeString = MacroMicroType.TIAMIN.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B2 Riboflavin
            total1 = eats.Sum(e => (e.Dish.VitaminB2Riboflavin ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB2Riboflavin ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB2Riboflavin ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB2Riboflavin ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.RIBOFLAVIN,
                TypeString = MacroMicroType.RIBOFLAVIN.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B3 Niacin
            total1 = eats.Sum(e => (e.Dish.VitaminB3Niacin ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB3Niacin ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB3Niacin ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB3Niacin ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.NIACIN,
                TypeString = MacroMicroType.NIACIN.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B6
            total1 = eats.Sum(e => (e.Dish.VitaminB6 ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB6 ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB6 ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB6 ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_B6,
                TypeString = MacroMicroType.VITAMIN_B6.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN B9 Folate
            total1 = eats.Sum(e => (e.Dish.VitaminB9Folate ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB9Folate ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB9Folate ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB9Folate ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.FOLATE,
                TypeString = MacroMicroType.FOLATE.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN C
            total1 = eats.Sum(e => (e.Dish.VitaminC ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminC ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminC ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminC ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_C,
                TypeString = MacroMicroType.VITAMIN_C.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN D
            total1 = eats.Sum(e => (e.Dish.VitaminD ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminD ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminD ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminD ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_D,
                TypeString = MacroMicroType.VITAMIN_D.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN E
            total1 = eats.Sum(e => (e.Dish.VitaminE ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminE ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminE ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminE ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_E,
                TypeString = MacroMicroType.VITAMIN_E.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //VITAMIN K
            total1 = eats.Sum(e => (e.Dish.VitaminK ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminK ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminK ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminK ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.VITAMIN_K,
                TypeString = MacroMicroType.VITAMIN_K.GetDescription()
            });
            max = 0.0;
            total1 = 0.0;
            total2 = 0.0;
            min = 1000000000.0;
            avg1 = 0.0;
            avg2 = 0.0;

            //ZINC
            total1 = eats.Sum(e => (e.Dish.Zinc ?? 0.0) * e.Qty);
            total2 = compoundEats.Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Zinc ?? 0.0) * d.DishQty)) * e.Qty);

            foreach (var group in groups1)
            {
                // get day value
                var temp = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Zinc ?? 0.0) * e.Qty);
                if (groups2.Any(g => g.Key == group.Key))
                {
                    // add compound dish if exists to the day total
                    temp += compoundEats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Zinc ?? 0.0) * d.DishQty)) * e.Qty);
                }
                // update max if greather than
                if (temp > max)
                {
                    max = temp;
                }
                // update min if less than
                if (temp < min)
                {
                    min = temp;
                }
            }

            avg1 = groups1.Count() > 0 ? total1 / groups1.Count() : 0.0;
            avg2 = groups2.Count() > 0 ? total2 / groups2.Count() : 0.0;

            list.Add(new MacroMicroValues
            {
                Avg = Math.Round((avg1 + avg2) / 2, 2),
                Max = max,
                Min = min,
                Total = total1 + total2,
                Type = MacroMicroType.ZINC,
                TypeString = MacroMicroType.ZINC.GetDescription()
            });

            return list;
        }

        private string GetImage()
        {
            var currentAssembly = typeof(ReportService).GetTypeInfo().Assembly;
            var root = Path.GetDirectoryName(currentAssembly.Location);
            var p = root + "\\imgs\\planifive.png";
            return p;
        }

        private async Task<byte[]> GetCoverContent(User user)

        {
            var info = await _pollService.GetUserPollsInfoAsync(user.Id);
            var currentWeekString = GetCurrentWeekRangeString();
            var currentWeekDates = GetCurrentWeekRangeDates();
            //var firstDay = currentWeekDates.ElementAt(0);
            //var lastDay = currentWeekDates.ElementAt(6);

            var firstDay = DateTime.Parse("Nov 15, 2020");
            var lastDay = DateTime.Parse("Nov 21, 2020");

            var eatsWeek = await _uow.EatDishRepository.GetAll().Where(ed => ed.Eat.CreatedAt.Date >= firstDay.Date &&
                    ed.Eat.CreatedAt.Date <= lastDay.Date &&
                    ed.Eat.UserId == user.Id)
                .Include(ed => ed.Dish)
                .Include(ed => ed.Eat)
                .ToListAsync();
            var eatsWeekCompoundDish = await _uow.EatCompoundDishRepository.GetAll().Where(ed => ed.Eat.CreatedAt.Date >= firstDay.Date &&
                    ed.Eat.CreatedAt.Date <= lastDay.Date &&
                    ed.Eat.UserId == user.Id)
                .Include(ed => ed.CompoundDish)
                    .ThenInclude(c => c.DishCompoundDishes)
                        .ThenInclude(d => d.Dish)
                .Include(ed => ed.Eat)
                .ToListAsync();

            var data = GetData(eatsWeek, eatsWeekCompoundDish);
            var dataMacro = data.Where(d => d.Type == MacroMicroType.PROTEINS ||
                         d.Type == MacroMicroType.CARBOHIDRATES ||
                         d.Type == MacroMicroType.FAT ||
                         d.Type == MacroMicroType.FAT_ACID_MONOINSATURATE ||
                         d.Type == MacroMicroType.FAT_ACID_POLYINSATURATE ||
                         d.Type == MacroMicroType.FAT_ACID_SATURATE
                         ).ToList();

            var dataMicro = data.Where(d => d.Type != MacroMicroType.PROTEINS &&
                        d.Type != MacroMicroType.CARBOHIDRATES &&
                        d.Type != MacroMicroType.FAT &&
                        d.Type != MacroMicroType.FAT_ACID_MONOINSATURATE &&
                        d.Type != MacroMicroType.FAT_ACID_POLYINSATURATE &&
                        d.Type != MacroMicroType.FAT_ACID_SATURATE
                        ).ToList();

            return new PdfReport().DocumentPreferences(doc =>
            {
                doc.RunDirection(PdfRunDirection.LeftToRight);
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata { Author = "PlaniFive", Application = "PlaniFive", Keywords = "Informe, Nutricional, PlaniFive", Subject = "Informe Nutricional", Title = "Informe Nutricional" });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
            .DefaultFonts(fonts =>
            {
                fonts.Size(10);
                fonts.Color(System.Drawing.Color.Black);
            })
            .PagesHeader(header =>
            {
            })
            .PagesFooter(footer =>
            {
            })
            .MainTableTemplate(t => t.BasicTemplate(BasicTemplate.ProfessionalTemplate))
            .MainTablePreferences(table =>
            {
                table.ColumnsWidthsType(TableColumnWidthType.Relative);
                table.GroupsPreferences(new GroupsPreferences
                {
                    GroupType = GroupType.HideGroupingColumns,
                    RepeatHeaderRowPerGroup = false,
                    ShowOneGroupPerPage = false,
                    SpacingBeforeAllGroupsSummary = 0f,
                    NewGroupAvailableSpacingThreshold = 150,
                    SpacingAfterAllGroupsSummary = 0f,
                    ShowAllGroupsSummaryRow = false, // its default value is true,
                });
                table.SpacingAfter(0f);
                table.SpacingBefore(0f);
            })
            .MainTableDataSource(dataSource =>
            {
                dataSource.StronglyTypedList(dataMacro);
            })
            .MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<MacroMicroValues>(x => x.Type);
                    column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1);
                    column.Width(2);
                    column.HeaderCell("Macronutriente");
                    column.CalculatedField(
                    list =>
                    {
                        if (list == null) return string.Empty;
                        return list.GetValueOf<MacroMicroValues>(x => x.TypeString);
                    });
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<MacroMicroValues>(x => x.Total);

                    column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2);
                    column.Width(2);
                    column.HeaderCell("Cantidad Total");
                    column.CalculatedField(
                     list =>
                     {
                         if (list == null) return string.Empty;
                         var rounded = double.Parse(list.GetValueOf<MacroMicroValues>(x => x.Total).ToString());
                         return Math.Round(rounded, 2);
                     });
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<MacroMicroValues>(x => x.Avg);

                    column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3);
                    column.Width(2);
                    column.HeaderCell("Promedio Diario");
                    column.CalculatedField(
                     list =>
                     {
                         if (list == null) return string.Empty;
                         var rounded = double.Parse(list.GetValueOf<MacroMicroValues>(x => x.Avg).ToString());
                         return Math.Round(rounded, 2);
                     });
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<MacroMicroValues>(x => x.Max);

                    column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4);
                    column.Width(2);
                    column.HeaderCell("Extremo Superior");
                    column.CalculatedField(
                     list =>
                     {
                         if (list == null) return string.Empty;
                         var rounded = double.Parse(list.GetValueOf<MacroMicroValues>(x => x.Max).ToString());
                         return Math.Round(rounded, 2);
                     });
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<MacroMicroValues>(x => x.Min);

                    column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5);
                    column.Width(2);
                    column.HeaderCell("Extremo Inferior");
                    column.CalculatedField(
                     list =>
                     {
                         if (list == null) return string.Empty;
                         var rounded = double.Parse(list.GetValueOf<MacroMicroValues>(x => x.Min).ToString());
                         return Math.Round(rounded, 2);
                     });
                });
            })
            .MainTableEvents(events =>
            {
                events.DataSourceIsEmpty(message: "There is no data available to display.");
                events.DocumentClosing(args =>
                {
                });

                events.MainTableCreated(args =>
                {
                    var marginTop = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100
                    };
                    marginTop.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "PlaniFive Premium - Informe Nutricional";

                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                         });
                    marginTop.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             //cellData.Value = "Semana de: Oct 10 - Oct 17";
                             cellData.Value = "Semana de: " + currentWeekString;

                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                         });
                    var tabTemp = marginTop.AddBorderToTable(borderColor: BaseColor.White, spacingBefore: 5f);
                    tabTemp.SpacingAfter = 5f;
                    args.PdfDoc.Add(tabTemp);

                    // image and title table
                    PdfPTable tableImage = new PdfPTable(1);
                    tableImage.SpacingBefore = 50f;
                    tableImage.HorizontalAlignment = Element.ALIGN_CENTER;
                    tableImage.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                    Image img = Image.GetInstance(GetImage());
                    img.ScaleAbsolute(200f, 200f);

                    var cell = new PdfPCell(img);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Border = Cell.NO_BORDER;
                    tableImage.AddCell(cell);
                    tableImage.SpacingAfter = 50f;
                    args.PdfDoc.Add(tableImage);

                    Table tabb = new Table(2, 6);
                    tabb.Alignment = 1;
                    tabb.DefaultHorizontalAlignment = 1;
                    //tabb.Padding = 5.0f;
                    tabb.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    tabb.DefaultCellBorder = Rectangle.NO_BORDER;
                    tabb.Width = 60;

                    int[] columnWidths = { 13, 21 };
                    tabb.SetWidths(columnWidths);

                    Font fontTitle = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.Black);
                    Paragraph name = new Paragraph("Usuario:", fontTitle);
                    Paragraph age = new Paragraph("Edad:", fontTitle);
                    Paragraph gender = new Paragraph("Sexo:", fontTitle);
                    Paragraph height = new Paragraph("Talla:", fontTitle);
                    Paragraph weight = new Paragraph("Peso:", fontTitle);
                    Paragraph imc = new Paragraph("IMC:", fontTitle);

                    Cell cellName = new Cell(name);
                    cellName.VerticalAlignment = Element.ALIGN_LEFT;
                    cellName.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    Cell cellAge = new Cell(age);
                    cellAge.VerticalAlignment = Element.ALIGN_LEFT;
                    cellAge.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    Cell cellGender = new Cell(gender);
                    cellGender.VerticalAlignment = Element.ALIGN_LEFT;
                    cellGender.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    Cell cellHeight = new Cell(height);
                    cellHeight.VerticalAlignment = Element.ALIGN_LEFT;
                    cellHeight.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    Cell cellWeight = new Cell(weight);
                    cellWeight.VerticalAlignment = Element.ALIGN_LEFT;
                    cellWeight.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    Cell cellImc = new Cell(imc);
                    cellImc.VerticalAlignment = Element.ALIGN_LEFT;
                    cellImc.HorizontalAlignment = Element.ALIGN_MIDDLE;

                    tabb.AddCell(cellName, 0, 0);
                    tabb.AddCell(cellAge, 1, 0);
                    tabb.AddCell(cellGender, 2, 0);
                    tabb.AddCell(cellHeight, 3, 0);
                    tabb.AddCell(cellWeight, 4, 0);
                    tabb.AddCell(cellImc, 5, 0);

                    Font fontHeaderColumTwo = FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.Black);

                    Paragraph phName = new Paragraph(user.Username, fontHeaderColumTwo);
                    Paragraph phAge = new Paragraph(info.age.ToString(), fontHeaderColumTwo);
                    Paragraph phGender = new Paragraph(info.sex == 1 ? "Masculino" : "Femenino", fontHeaderColumTwo);
                    Paragraph phHeight = new Paragraph(info.height.ToString(), fontHeaderColumTwo);
                    Paragraph phWeight = new Paragraph(info.weight.ToString(), fontHeaderColumTwo);
                    Paragraph phImc = new Paragraph(Math.Round(user.CurrentImc, 2).ToString(), fontHeaderColumTwo);

                    Cell cellValueName = new Cell(phName);
                    cellValueName.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueName.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Cell cellValueAge = new Cell(phAge);
                    cellValueAge.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueAge.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Cell cellValueGender = new Cell(phGender);
                    cellValueGender.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueGender.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Cell cellValueHeight = new Cell(phHeight);
                    cellValueHeight.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueHeight.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Cell cellValueWeight = new Cell(phWeight);
                    cellValueWeight.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueWeight.HorizontalAlignment = Element.ALIGN_RIGHT;

                    Cell cellValueImc = new Cell(phImc);
                    cellValueImc.VerticalAlignment = Element.ALIGN_RIGHT;
                    cellValueImc.HorizontalAlignment = Element.ALIGN_RIGHT;

                    tabb.AddCell(cellValueName, 0, 1);
                    tabb.AddCell(cellValueAge, 1, 1);
                    tabb.AddCell(cellValueGender, 2, 1);
                    tabb.AddCell(cellValueHeight, 3, 1);
                    tabb.AddCell(cellValueWeight, 4, 1);
                    tabb.AddCell(cellValueImc, 5, 1);

                    args.PdfDoc.Add(tabb);
                    args.PdfDoc.NewPage();

                    AddChartToPage(args.PdfDoc, eatsWeek, eatsWeekCompoundDish);
                    args.PdfDoc.NewPage();

                    var macroInfo = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100,
                        SpacingAfter = 20
                    };
                    macroInfo.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "Ha ingerido usted de forma aproximada según lo reportado en nuestra App los siguientes macronutrientes:";

                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             properties.PaddingBottom = 10;
                         });
                    args.PdfDoc.Add(macroInfo);
                });

                events.MainTableAdded(args =>
                {
                    //Show Data after the content table.
                    var macroInfo = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 70,
                        SpacingAfter = 30
                    };
                    macroInfo.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "Ha ingerido usted de forma aproximada según lo reportado en nuestra App los siguientes micronutrientes:";

                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                         });
                    args.PdfDoc.Add(macroInfo);

                    var tabb = new PdfGrid(5)
                    {
                        WidthPercentage = 100
                    };
                    tabb.AddSimpleRow((cellData, cellProperties) =>
                    {
                        cellData.Value = "Micronutriente";
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.PdfFont = args.PdfFont;
                        cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                        cellProperties.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#5D7B9D").ToArgb());
                        cellProperties.FontColor = BaseColor.White;
                    }, (cellData, cellProperties) =>
                    {
                        cellData.Value = "Cantidad Total";
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.PdfFont = args.PdfFont;
                        cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                        cellProperties.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#5D7B9D").ToArgb());
                        cellProperties.FontColor = BaseColor.White;
                    }, (cellData, cellProperties) =>
                    {
                        cellData.Value = "Promedio Diario";
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.PdfFont = args.PdfFont;
                        cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                        cellProperties.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#5D7B9D").ToArgb());
                        cellProperties.FontColor = BaseColor.White;
                    }, (cellData, cellProperties) =>
                    {
                        cellData.Value = "Extremo Superior";
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.PdfFont = args.PdfFont;
                        cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                        cellProperties.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#5D7B9D").ToArgb());
                        cellProperties.FontColor = BaseColor.White;
                    }, (cellData, cellProperties) =>
                    {
                        cellData.Value = "Extremo Inferior";
                        cellProperties.PdfFont = args.PdfFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                        cellProperties.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#5D7B9D").ToArgb());
                        cellProperties.FontColor = BaseColor.White;
                    }

                        );

                    var count = 0;
                    foreach (var item in dataMicro)
                    {
                        tabb.AddSimpleRow((cellData, cellProperties) =>
                        {
                            cellData.Value = item.TypeString;
                            cellProperties.PdfFont = args.PdfFont;
                            cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            cellProperties.BackgroundColor = count % 2 == 0 ? new BaseColor(System.Drawing.Color.White.ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#F7F6F3").ToArgb());
                            cellProperties.FontColor = count % 2 == 0 ? new BaseColor(System.Drawing.ColorTranslator.FromHtml("#284775").ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#333333").ToArgb());
                        }, (cellData, cellProperties) =>
                        {
                            cellData.Value = Math.Round(item.Total, 2).ToString();
                            cellProperties.PdfFont = args.PdfFont;
                            cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            cellProperties.BackgroundColor = count % 2 == 0 ? new BaseColor(System.Drawing.Color.White.ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#F7F6F3").ToArgb());
                            cellProperties.FontColor = count % 2 == 0 ? new BaseColor(System.Drawing.ColorTranslator.FromHtml("#284775").ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#333333").ToArgb());
                        }, (cellData, cellProperties) =>
                        {
                            cellData.Value = Math.Round(item.Avg, 2).ToString();
                            cellProperties.PdfFont = args.PdfFont;
                            cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            cellProperties.BackgroundColor = count % 2 == 0 ? new BaseColor(System.Drawing.Color.White.ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#F7F6F3").ToArgb());
                            cellProperties.FontColor = count % 2 == 0 ? new BaseColor(System.Drawing.ColorTranslator.FromHtml("#284775").ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#333333").ToArgb());
                        }, (cellData, cellProperties) =>
                        {
                            cellData.Value = Math.Round(item.Max, 2).ToString();
                            cellProperties.PdfFont = args.PdfFont;
                            cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            cellProperties.BackgroundColor = count % 2 == 0 ? new BaseColor(System.Drawing.Color.White.ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#F7F6F3").ToArgb());
                            cellProperties.FontColor = count % 2 == 0 ? new BaseColor(System.Drawing.ColorTranslator.FromHtml("#284775").ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#333333").ToArgb());
                        }, (cellData, cellProperties) =>
                        {
                            cellData.Value = Math.Round(item.Min, 2).ToString();
                            cellProperties.PdfFont = args.PdfFont;
                            cellProperties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            cellProperties.BackgroundColor = count % 2 == 0 ? new BaseColor(System.Drawing.Color.White.ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#F7F6F3").ToArgb());
                            cellProperties.FontColor = count % 2 == 0 ? new BaseColor(System.Drawing.ColorTranslator.FromHtml("#284775").ToArgb()) : new BaseColor(System.Drawing.ColorTranslator.FromHtml("#333333").ToArgb());
                        });
                        count += 1;
                    }
                    args.PdfDoc.Add(tabb);

                    var gr = eatsWeek.GroupBy(e => e.Eat.CreatedAt.Date);
                    var gr1 = eatsWeekCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date);
                    var uniqueTimes = new List<DateTime>();
                    foreach (var group in gr)
                    {
                        if (!uniqueTimes.Any(u => u.Date == group.Key.Date))
                        {
                            uniqueTimes.Add(group.Key);
                        }
                    }

                    if (uniqueTimes.Count != 7)
                    {
                        var alert = new PdfGrid(numColumns: 1)
                        {
                            WidthPercentage = 100,
                            SpacingBefore = 70
                            //SpacingAfter = 30
                        };
                        alert.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Nota: Para una mejor interpretación de los datos es aconsejable planificar su alimentación cada día.";
                                 properties.PdfFont = events.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PdfFontStyle = DocumentFontStyle.BoldItalic;
                             });
                        args.PdfDoc.Add(alert);
                    }
                    args.PdfDoc.NewPage();

                    var tips = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100
                    };
                    tips.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "Consejos nutricionales de la semana:";
                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             properties.PaddingBottom = 20;
                             properties.PdfFontStyle = DocumentFontStyle.Underline;
                         });
                    if (ProteinsAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Proteínas:";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PdfFontStyle = DocumentFontStyle.Underline;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Incremente el consumo de proteínas. Trate de balancear entre las proteínas de origen animal y vegetal. " +
                                 "Si combina ambas en el mismo plato aumenta la biodisponibilidad. " +
                                 "Recuerde que las proteínas de origen animal tienen aminoácidos esenciales que no se incorporan al organismo de otra forma. " +
                                 "Son una fuente muy importante de aporte de proteínas los lácteos y sus derivados.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (CarbohidratosMINAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Carbohidratos:";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PdfFontStyle = DocumentFontStyle.Underline;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe ingerir un mínimo de 130g por día de carbohidratos debido a que es la cantidad mínima recomendada " +
                                 "para que el cerebro funcione correctamente.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se recomienda que un 75% del total de los carbohidratos ingeridos sean complejos, como por ejemplo almidones " +
                                 "que se encuentran en cereales, raíces y tubérculos (chivirias, papa), legumbres, frutas ricas en almidón (plátano, calabaza, calabacín, maíz, arvejas o guisantes) " +
                                 "y un 25 % de carbohidratos simples, preferir frutas crudas.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    else if (CarbohidratosMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Carbohidratos:";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PdfFontStyle = DocumentFontStyle.Underline;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe disminuir el consumo de carbohidratos diarios pues todo el exceso se almacena en forma de grasa en el organismo.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se recomienda que un 75% del total de los carbohidratos ingeridos sean complejos, como por ejemplo almidones " +
                                 "que se encuentran en cereales, raíces y tubérculos (chivirias, papa), legumbres, frutas ricas en almidón (plátano, calabaza, calabacín, maíz, arvejas o guisantes) " +
                                 "y un 25 % de carbohidratos simples, preferir frutas crudas.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (TotalFatsMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Grasas totales:";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PdfFontStyle = DocumentFontStyle.Underline;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se recomienda disminuir la cantidad de alimentos que contienen grasa.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se recomienda ingerir menor cantidad de ac. grasos saturados por lo que disminuya o evite la bollería sobre todo industrial, " +
                                 "comidas de fast food que emplean aceites vegetales solidificados; dulces y manteca de coco, mantecas animales, carnes y derivados, embutidos, etc.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Haga un balance adecuado de las grasas que ingiere, los ácidos grasos monoinsaturados como el oleico, está presente de forma " +
                                 "abundante en los aceites de oliva y de colza y en diversas frutas y frutos secos (aguacate, avellanas, cacahuete, almendras, nueces y otros). " +
                                 "Es más beneficiosos para su salud que la ingestión de grasas saturadas.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Incremente la ingestión de grasas polinsaturadas. La ingestión deseable de ácido linoleico debe proveer como mínimo " +
                                 "el 5 % de la energía. Es un ac. graso esencial o sea no se fabrica en el organismo humano por lo que hay que tomarlo de los alimentos. " +
"La fuente dietética principal de ácidos grasos polinsaturados de la serie n-6 (procedente del ácido linoleico) es el consumo de aceites vegetales" +
"(girasol, soja, maíz y sus derivados no hidrogenados), aunque también se encuentran, en menor cantidad, en otros alimentos como la leche, ciertos frutos secos, " +
"los aguacates y otros." +
"Los ácidos grasos polinsaturados de la serie n-3 más importantes en nutrición humana son el ácido eicosapentaenoico y el docosahexaenoico, abundantes en las grasas y aceites procedentes del pescado sobre todo los azules y el α-linolénico " +
"(presente en diversos alimentos vegetales como los aceites de semilla de soja, semilla de lino, nueces o colza).";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    else if (SaturatedFatsMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Ac. grasos saturados:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se recomienda ingerir menor cantidad de ac. grasos saturados por lo que disminuya o evite la bollería sobre todo industrial, " +
                                 "comidas de fast food que emplean aceites vegetales solidificados; dulces y manteca de coco, mantecas animales, carnes y derivados, embutidos, etc.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    else if (MonoInsaturatedFatsMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Ac. grasos monoinsaturados:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Haga un balance adecuado de las grasas que ingiere, los ácidos grasos monoinsaturados como el oleico, está presente de forma " +
                                 "abundante en los aceites de oliva y de colza y en diversas frutas y frutos secos (aguacate, avellanas, cacahuete, almendras, nueces y otros). " +
                                 "Es más beneficiosos para su salud que la ingestión de grasas saturadas.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    else if (PolyInsaturatedFatsMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Ac. grasos polinsaturados:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Incremente la ingestión de grasas polinsaturadas. La ingestión deseable de ácido linoleico debe proveer como mínimo " +
                                 "el 5 % de la energía. Es un ac. graso esencial o sea no se fabrica en el organismo humano por lo que hay que tomarlo de los alimentos. " +
"La fuente dietética principal de ácidos grasos polinsaturados de la serie n-6 (procedente del ácido linoleico) es el consumo de aceites vegetales" +
"(girasol, soja, maíz y sus derivados no hidrogenados), aunque también se encuentran, en menor cantidad, en otros alimentos como la leche, ciertos frutos secos, " +
"los aguacates y otros." +
"Los ácidos grasos polinsaturados de la serie n-3 más importantes en nutrición humana son el ácido eicosapentaenoico y el docosahexaenoico, abundantes en las grasas y aceites procedentes del pescado sobre todo los azules y el α-linolénico " +
"(presente en diversos alimentos vegetales como los aceites de semilla de soja, semilla de lino, nueces o colza).";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (CholesterolMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Colesterol:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe disminuir la ingesta de colesterol. Algunas de las principales fuentes son carnes, grasas animales, lácteos y derivados y la yema del huevo.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (FiberMINAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Fibra:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Incremente el consumo de fibra dietética que puede encontrar en cereales integrales y raíces, legumbres, verduras (hortalizas) y frutas. " +
                                 "Debe tomar diariamente al menos seis raciones de derivados de cereales, tres de verduras y dos de fruta para incrementar la fibra.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (AlcoholMAXAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Alcohol:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Disminuya la cantidad de alcohol ingerida, trate de no beber alcohol diariamente.";

                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminAMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina A:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: hígado, aceites de pescado, huevo, leche entera y productos lácteos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen vegetal: zanahoria, espinaca, lechuga, fruta bomba, mango, calabaza, malanga amarilla, yuca amarilla, boniato amarillo, vegetales de hojas verdes, brócoli, espárragos plátano, maíz, tomate, naranjas, mandarinas guayaba, caqui, fruta de la pasión y melocotón.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Evitar exponer estos alimentos a procesos de cocción que pueden destruirla pues es muy sensible a oxidación por luz, calor, aire, acidez y humedad.La freidura es el proceso que más la destruye.Se puede proteger con antioxidantes en combinación por ejemplo ricos en Vitamina C.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminDMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina D:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: aceite de hígado de pescado, pescados grasos(salmón, arenque, anguila), pescado en conserva, yema de huevo, carnes rojas, hígado, mantequilla y queso crema. Una fuente adicional es la luz solar por lo que es recomendable exponerse unos 10 minutos diarios al sol.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Evitar exponer estos alimentos al aire y a la luz solar.Tiene relativa estabilidad al calor(hasta 150 ºC). Tolera bien el almacenamiento.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (VitaminEMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina E:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Aceites vegetales (germen de trigo, oliva, girasol, soya, maíz, maní), mantequilla, huevo entero, arroz integral, harina de maíz, grasa para cocinar y mayonesa, germen de trigo, nueces, pipas, pistachos, cacahuetes y cereales integrales.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Es una vitamina muy sensible al calor. Es inestable cuando se expone al aire y la luz, pero estable al tratamiento térmico en ausencia de oxígeno. La vitamina E se destruye cuando ejerce su función antioxidante. Se destruye en aceites rancios.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminKMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina K:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: leche, hígado (especialmente de cerdo).";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen vegetal: en vegetales de color verde intenso, chícharos, judías verdes, aceites de soya, colza y oliva.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: De forma habitual la síntesis de (menaquinona) se realiza por bacterias intestinales por lo que disminuye su formación con el tratamiento antibiótico oral que erradica flora intestinal." +
"Es inestable en medio ácido y luz, en el refinado de aceites.Precaución los pacientes en tto con dicumarol y warfarina.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminCMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina C:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen vegetal: Frutas como acerola, guayaba, marañón, cítricos, melón de agua o sandía, melón de Castilla, uva, mango, papaya, fresa, toronja, piña y vegetales como pimiento, col, brócoli, col de Bruselas, col blanca, coliflor, ají pimiento rojo y verde, tomate.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Es una vitamina inestable en soluciones neutras y alcalinas y cuando se expone al aire, luz, calor, humedad, a cobre o hierro, perdidas por lixiviación en alimentos expuestos o troceados, pérdidas por tratamientos con aspirina, barbitúricos y antihistamínicos o tabaco La piel de las viandas la protege durante la cocción.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (FolatosMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Folatos:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: hígado, carnes, huevo entero, ostras, Origen vegetal: leguminosas, cereales integrales, levaduras, vegetales de hojas, viandas como papa, calabaza y boniato, vegetales como quimbombó, berro, nabos, pimientos y tomates, frutas como plátanos, cítricos y melón, nueces y frutos secos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Es inestable en soluciones ácidas y cuando se expone al calor, al aire y a la luz. Pérdida por lixiviación en alimentos expuestos. Pérdida por cocción.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminB1MinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina B1 (Tiamina):";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Cereales integrales, levaduras, productos cárnicos (cerdo, hígado, corazón y riñones), legumbres, verduras, viandas y semillas o nueces, leche, frutas y huevos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Inestable en álcalis, aire, luz, se pierde 100% por pulido de cereales, 50% por cocción por lixiviación, por tiaminasas de peces, moluscos y vegetales. La ingesta mantenida de alcohol incrementa las necesidades de esta vitamina.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminB2MinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina B2 (Riboflavina):";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: productos cárnicos que incluyen aves y pescados, huevos, leche y Origen vegetal: leguminosas, vegetales de hojas (col de brócoli y espinacas), levaduras, cereales no refinados.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Muy inestable en álcalis y a la luz solar, artificial y UV. Pérdidas por lixiviación.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (NiacinMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Niacina:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: las carnes (res, cerdo, pollo) vísceras (hígado), leche y derivados, huevos, embutidos, pescado, Origen vegetal: leguminosas, cereales no refinados, legumbres, semillas (maní), vegetales de hojas verdes, café, té y levaduras.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Es bastante estable en medio externo. Pérdidas por lixiviación y refinado de cereales, Incremento de necesidades por agotamiento de reservas en alcoholismo.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (VitaminB6MinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina B6 (Piridoxina):";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: carnes (fresca, cerdo, pollo, embutidos, pescado, vísceras (riñones, hígado), huevos. Origen vegetal: arroz integral, la soya, cebada, productos de trigo entero, maní, nueces y vegetales de color verde.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Se pierde por calentamiento o almacenamiento prolongado. Es inestable cuando se expone a la luz y calor por lixiviación.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (VitaminB12MinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Vitamina B12 (Cianocobalamina):";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });
                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: carne y productos cárnicos (mariscos, pescados, aves), huevos, vísceras (hígado, riñones y corazón), bivalvos como almejas y ostras, yema de huevo, cangrejos, róbalo, salmón y sardinas, leche en polvo desgrasada y quesos líquidos y fermentados. Algunos microorganismos del colon sintetizan B12.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: Es inestable al aire y la luz. La leche hervida 2-5 minutos pierde un 30% de vit B12. Leche procesada es una fuente insuficiente de esta vitamina.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    if (SodiumMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Sodio:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe disminuir la ingesta de sodio.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Los alimentos que contienen más sodio son aquellos que sufren procesos de salado y curación, como jamones, cecinas, embutidos, chacinas, pescados en salazón y, quesos duros, vegetales en conserva, pan y salsas listas preelaboradas. Los alimentos naturales como el apio, mariscos (ostras y ostiones).";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (PotassiumMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Potasio:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Deben tener precaución con el exceso de potasio las personas con patologías crónicas sobre todo renales.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen vegetal: vegetales de hoja verde (espinaca, lechuga, acelga), col, tomate, pepino, berenjena, calabaza, zanahoria, rábano, nabo, cebolla, frijoles, chícharo, habichuela, garbanzo, naranja, toronja, plátano fruta, papa, boniato, ñame.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Origen animal: leche entera, leche descremada, yogurt.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (CalciumMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Calcio:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar el consumo de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Productos lácteos, col, brócoli, semillas de ajonjolí, huesos blandos de peces (sardinas, salmón), huesos de las patas de pollos, alimentos fortificados con calcio. El agua es una fuente variable.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (PhosphorusMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Fósforo:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Se encuentra en casi todos los alimentos. Los alimentos proteicos y los cereales son ricos en este mineral.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (IronMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Hierro:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Carnes y derivados que contengan hemo ( carnes rojas), pollo. Son fuentes aceptables los huevos, cereales y vegetales de hoja verde.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "NOTA: La ingestión simultánea de 25 a 100 mg de vitamina C puede incrementar en 2-4 veces la absorción del hierro no hemínico.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }
                    if (ZincMinAdviseIsNeeded(info, eatsWeek, eatsWeekCompoundDish))
                    {
                        tips.AddSimpleRow(
                                 (cellData, properties) =>
                                 {
                                     cellData.Value = "Zinc:";

                                     properties.PdfFont = args.PdfFont;
                                     properties.RunDirection = PdfRunDirection.LeftToRight;
                                     properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                     properties.PdfFontStyle = DocumentFontStyle.Underline;
                                 });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Debe incrementar la ingesta de alimentos.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                             });

                        tips.AddSimpleRow(
                             (cellData, properties) =>
                             {
                                 cellData.Value = "Mariscos, carnes rojas, vísceras, semillas, germen de trigo, carne de aves, cerdo, legumbres, yogurt, huevo, quesos, nueces, vegetales de hojas.";
                                 properties.PdfFont = args.PdfFont;
                                 properties.RunDirection = PdfRunDirection.LeftToRight;
                                 properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                                 properties.PaddingBottom = 10f;
                             });
                    }

                    args.PdfDoc.Add(tips);

                    var bye = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 200
                    };
                    bye.AddSimpleRow(
                        (cellData, properties) =>
                        {
                            cellData.Value = "Dra. Saira R. Rivas Suárez";

                            properties.PdfFont = args.PdfFont;
                            properties.RunDirection = PdfRunDirection.LeftToRight;
                            properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            properties.PdfFontStyle = DocumentFontStyle.Bold;
                        });

                    args.PdfDoc.Add(bye);
                    var bye1 = new PdfGrid(numColumns: 1)
                    {
                        WidthPercentage = 100,
                        SpacingBefore = 200
                    };

                    bye1.AddSimpleRow(
                        (cellData, properties) =>
                        {
                            cellData.Value = "Nota: Este informe no constituye un documento clínico.";

                            properties.PdfFont = events.PdfFont;
                            properties.RunDirection = PdfRunDirection.LeftToRight;
                            properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                            properties.PdfFontStyle = DocumentFontStyle.Bold;
                            properties.FontColor = BaseColor.Red;
                        });

                    args.PdfDoc.Add(bye1);
                });

                events.ShouldSkipRow(args =>
                {
                    return false;
                });

                var pageNumber = 0;
                events.ShouldSkipHeader(args =>
                {
                    pageNumber++;
                    if (pageNumber == 2)
                    {
                        return true; // don't render this header row.
                    }

                    return false;
                });

                events.ShouldSkipFooter(args =>
                {
                    if (pageNumber == 2)
                    {
                        return true; // don't render this footer row.
                    }

                    return false;
                });
            })

            .GenerateAsByteArray();
        }

        private bool ZincMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var zinc = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Zinc ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    zinc += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Zinc ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // masculino
                if (info.sex == 1 && zinc < 14)
                {
                    return true;
                }
                //femenino
                else
                {
                    if (zinc < 10)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IronMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var iron = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Iron ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    iron += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Iron ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // masculino
                if (info.sex == 1 && iron < 14)
                {
                    return true;
                }
                //femenino
                else
                {
                    if (info.age < 60 && iron < 20)
                    {
                        return true;
                    }
                    else if (iron < 12)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool PhosphorusMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var phos = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Phosphorus ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    phos += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Phosphorus ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (phos < 800)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CalciumMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var calcium = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calcium ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    calcium += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calcium ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (calcium < 800)
                {
                    return true;
                }
            }
            return false;
        }

        private bool PotassiumMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var potassium = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Potassium ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    potassium += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Potassium ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (potassium > 3500)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SodiumMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var sodium = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Sodium ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    sodium += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Sodium ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (sodium > 2300)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminB12MinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminB12 = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB12 ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminB12 += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB12 ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (vitaminB12 < 2.4)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminB6MinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminB6 = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB6 ?? 0.0) * e.Qty);
                var proteins = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Proteins ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminB6 += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB6 ?? 0.0) * d.DishQty)) * e.Qty);
                    proteins += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (vitaminB6 < (proteins * 0.02))
                {
                    return true;
                }
            }
            return false;
        }

        private bool NiacinMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var niacin = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB3Niacin ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    niacin += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB3Niacin ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (niacin < 7)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminB2MinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminB2 = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB2Riboflavin ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminB2 += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB2Riboflavin ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (vitaminB2 < 1.2)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminB1MinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminB1 = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB1Thiamin ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminB1 += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB1Thiamin ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (vitaminB1 < 1)
                {
                    return true;
                }
            }
            return false;
        }

        private bool FolatosMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var folatos = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminB9Folate ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    folatos += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminB9Folate ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (folatos < 400)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminCMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminC = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminC ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminC += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminC ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (info.sex != 1 && vitaminC < 75)
                {
                    return true;
                }
                else if (info.sex == 1 && vitaminC < 90)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminKMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminK = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminK ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminK += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminK ?? 0.0) * d.DishQty)) * e.Qty);
                }

                // femenino
                if (info.sex != 1 && vitaminK < 55)
                {
                    return true;
                }
                else if (info.sex == 1 && vitaminK < 120)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminEMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminE = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminE ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminE += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminE ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (vitaminE < 15)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminDMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminD = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminD ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminD += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminD ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (info.age < 50 && vitaminD < 5)
                {
                    return true;
                }
                else if (info.age > 50 && info.age < 65 && vitaminD < 10)
                {
                    return true;
                }
                else if (info.age > 65 && vitaminD < 15)
                {
                    return true;
                }
            }
            return false;
        }

        private bool VitaminAMinAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var vitaminA = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.VitaminA ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    vitaminA += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.VitaminA ?? 0.0) * d.DishQty)) * e.Qty);
                }

                //femenino
                if (info.sex != 1 && vitaminA < 500)
                {
                    return true;
                }
                //masculino
                else
                {
                    if (vitaminA < 600)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool AlcoholMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var alcohol = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Alcohol ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    alcohol += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Alcohol ?? 0.0) * d.DishQty)) * e.Qty);
                }

                var ube = alcohol / 13;
                //femenino
                if (info.sex != 1)
                {
                    if (ube > 2)
                    {
                        return true;
                    }
                }
                //masculino
                else
                {
                    if (ube > 4)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool FiberMINAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var fiber = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Fiber ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    fiber += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fiber ?? 0.0) * d.DishQty)) * e.Qty);
                }

                //femenino
                if (info.sex != 1)
                {
                    if (info.age < 50 && fiber < 25)
                    {
                        return true;
                    }
                    else if (info.age >= 50 && fiber < 21)
                    {
                        return true;
                    }
                }
                //masculino
                else
                {
                    if (info.age < 50 && fiber < 38)
                    {
                        return true;
                    }
                    else if (info.age >= 50 && fiber < 30)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool CholesterolMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var cholesterol = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Cholesterol ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    cholesterol += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Cholesterol ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (cholesterol > 300)
                {
                    return true;
                }
            }
            return false;
        }

        private bool PolyInsaturatedFatsMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var fats = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.PolyUnsaturatedFat ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    fats += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.PolyUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (fats / calories > 0.07)
                {
                    return true;
                }
            }
            return false;
        }

        private bool MonoInsaturatedFatsMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var fats = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.MonoUnsaturatedFat ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    fats += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.MonoUnsaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (fats / calories > 0.15)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SaturatedFatsMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var fats = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.SaturatedFat ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    fats += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.SaturatedFat ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (fats / calories > 0.10)
                {
                    return true;
                }
            }
            return false;
        }

        private bool TotalFatsMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var fats = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Fat ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    fats += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Fat ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (fats / calories > 0.35)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CarbohidratosMAXAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var carbos = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Carbohydrates ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    carbos += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (carbos / calories > 0.70)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CarbohidratosMINAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var carbos = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Carbohydrates ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    carbos += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Carbohydrates ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (carbos < 130)
                {
                    return true;
                }
            }

            return false;
        }

        private bool ProteinsAdviseIsNeeded((int age, int weight, int height, int sex, DateTime? HealthMeasuresLastUpdate, DateTime? ValueMeasuresLastUpdate, DateTime? WellnessMeasuresLastUpdate, DateTime? LastPlanedEat) info, List<EatDish> eats, List<EatCompoundDish> eatsCompoundDish)
        {
            var gr = eats.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);
            var gr1 = eatsCompoundDish.GroupBy(e => e.Eat.CreatedAt.Date).OrderBy(g => g.Key);

            foreach (var group in gr)
            {
                var proteins = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Proteins ?? 0.0) * e.Qty);
                var calories = eats.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.Dish.Calories ?? 0.0) * e.Qty);
                if (gr1.Any(g => g.Key == group.Key))
                {
                    proteins += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Proteins ?? 0.0) * d.DishQty)) * e.Qty);
                    calories += eatsCompoundDish.Where(e => e.Eat.CreatedAt.Date == group.Key).Sum(e => (e.CompoundDish.DishCompoundDishes.Sum(d => (d.Dish.Calories ?? 0.0) * d.DishQty)) * e.Qty);
                }

                if (proteins / calories < 0.12)
                {
                    return true;
                }
            }

            return false;
        }

        private string GetCurrentWeekRangeString()
        {
            DateTime startOfWeek = DateTime.Today.AddDays((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)DateTime.Today.DayOfWeek);

            //string result = string.Join("," + Environment.NewLine, Enumerable.Range(0, 7)
            //  .Select(i => startOfWeek
            //     .AddDays(i)
            //     .ToString("MM dd")));
            var range = Enumerable.Range(0, 7).Select(i => startOfWeek
                    .AddDays(i)
                    .ToString("MMM dd, yyyy"));
            return range.ElementAt(0) + " - " + range.ElementAt(6);
        }

        private IEnumerable<DateTime> GetCurrentWeekRangeDates()
        {
            DateTime startOfWeek = DateTime.Today.AddDays((int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)DateTime.Today.DayOfWeek);

            var range = Enumerable.Range(0, 7).Select(i => startOfWeek
                    .AddDays(i));
            return range;
        }

        private byte[] GetChartsContent()
        {
            var data = GetData(null, null);
            try
            {
                return new PdfReport().DocumentPreferences(doc =>
                {
                    doc.RunDirection(PdfRunDirection.LeftToRight);
                    doc.Orientation(PageOrientation.Landscape);
                    doc.PageSize(PdfPageSize.A4);
                    doc.DocumentMetadata(new DocumentMetadata { Author = "PlaniFive", Application = "PlaniFive", Keywords = "Nutricional", Title = "Informe Nutricional" });
                    doc.Compression(new CompressionSettings
                    {
                        EnableCompression = true,
                        EnableFullCompression = true
                    });
                })
                 .DefaultFonts(fonts =>
                 {
                     fonts.Size(9);
                     fonts.Color(System.Drawing.Color.Black);
                 })
                 .PagesFooter(footer =>
                 {
                     footer.DefaultFooter(DateTime.Now.ToString("MMM. dd, yyyy"));
                 })
                 .PagesHeader(header =>
                 {
                     header.CacheHeader(cache: false);
                 })
                 .MainTableTemplate(template =>
                 {
                 })
                 .MainTablePreferences(table =>
                 {
                     table.ColumnsWidthsType(TableColumnWidthType.FitToContent);
                     table.NumberOfDataRowsPerPage(10);
                 })
                 .MainTableDataSource(dataSource =>
                 {
                     dataSource.StronglyTypedList(data);
                 })
                 .MainTableColumns(columns =>
                 {
                     columns.AddColumn(column =>
                     {
                         column.PropertyName("rowNo");
                         column.IsRowNumber(true);
                         column.CellsHorizontalAlignment(PdfRpt.Core.Contracts.HorizontalAlignment.Center);
                         column.IsVisible(true);
                         //column.Order(0);
                         column.Width(1);
                         column.HeaderCell("#");
                     });
                 })
                 .MainTableEvents(events =>
                 {
                     events.DataSourceIsEmpty(message: "There is no data available to display.");

                     events.DocumentClosing(args =>
                     {
                         //AddChartToPage(args.PdfDoc);
                         //AddChartToPage(args.PdfDoc, width: (int)args.PdfWriter.PageSize.Width - 100, height: 250);
                     });
                 })
                   .GenerateAsByteArray();
                //.Generate(data => data.AsPdfFile("d:\\NutriotionalReport.pdf"));
                //.Export(export => { });
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}