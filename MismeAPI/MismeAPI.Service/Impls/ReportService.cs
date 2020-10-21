using iTextSharp.text;
using iTextSharp.text.pdf;
using MismeAPI.Common.DTO;
using MismeAPI.Data.UoW;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MismeAPI.Service.Impls
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _uow;

        public ReportService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public Task GetFeedReportAsync()
        {
            throw new NotImplementedException();
        }

        public void GetNutritionalReport()
        {
            //var image = GetImage();
            var cover = GetCoverContent();
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
                                   float spacingBefore = 5,
                                   float spacingAfter = 5,
                                   float widthPercentage = 100)
        {
            var plt = new ScottPlot.Plot(600, 400);

            // create some sample data
            double[] xs = { 1, 2, 3, 4, 5, 6, 7 };
            double[] valuesA = { 1, 2, 3, 2, 1, 2, 1 };
            double[] valuesB = { 3, 3, 2, 1, 3, 2, 1 };

            // to simulate stacking B on A, shift B up by A
            double[] valuesB2 = new double[valuesB.Length];
            for (int i = 0; i < valuesB.Length; i++)
                valuesB2[i] = valuesA[i] + valuesB[i];

            // plot the bar charts in reverse order (highest first)
            plt.PlotBar(xs, valuesB2, label: "Series B");
            plt.PlotBar(xs, valuesA, label: "Series A");

            // improve the styling
            plt.Legend(location: legendLocation.upperRight);
            //plt.Title("Stacked Bar Charts");

            //var currentAssembly = typeof(ReportService).GetTypeInfo().Assembly;
            //var root = Path.GetDirectoryName(currentAssembly.Location);
            plt.SaveFig("PlotTypes_Bar_Stacked.png");

            var imageBytes = File.ReadAllBytes("PlotTypes_Bar_Stacked.png");
            var iTextSharpImage = PdfImageHelper.GetITextSharpImageFromByteArray(imageBytes);
            iTextSharpImage.Alignment = Element.ALIGN_CENTER;

            var tableImage = new PdfPTable(1);
            tableImage.HorizontalAlignment = Element.ALIGN_CENTER;
            tableImage.DefaultCell.Border = Rectangle.NO_BORDER;
            tableImage.WidthPercentage = 100;

            tableImage.AddCell(iTextSharpImage);

            var tableImage1 = new PdfPTable(1);
            tableImage1.HorizontalAlignment = Element.ALIGN_CENTER;
            tableImage1.DefaultCell.Border = Rectangle.NO_BORDER;
            tableImage1.WidthPercentage = 100;

            tableImage1.AddCell(iTextSharpImage);

            pdfDoc.Add(tableImage);
            pdfDoc.Add(tableImage1);
        }

        private List<MacroMicroValues> GetData()
        {
            var list = new List<MacroMicroValues>();
            for (int i = 0; i <= 25; i++)
            {
                list.Add(new MacroMicroValues
                {
                    Avg = 11.12 * i,
                    Max = 12.32 * i,
                    Min = 1.32 * i,
                    Total = 1551.32 * i,
                    Type = (MacroMicroType)i
                });
            }

            return list;
        }

        private string GetImage()
        {
            var currentAssembly = typeof(ReportService).GetTypeInfo().Assembly;
            var root = Path.GetDirectoryName(currentAssembly.Location);
            var p = root + "\\imgs\\planifive.png";
            return p;
        }

        private byte[] GetCoverContent()
        {
            var data = GetData();
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
                        return list.GetValueOf<MacroMicroValues>(x => x.Type).ToString();
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
                             cellData.Value = "Semana de: Oct 10 - Oct 17";

                             properties.PdfFont = events.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Center;
                         });
                    var tabTemp = marginTop.AddBorderToTable(borderColor: BaseColor.White, spacingBefore: 5f);
                    tabTemp.SpacingAfter = 5f;
                    args.PdfDoc.Add(tabTemp);

                    // image and title table
                    PdfPTable tableImage = new PdfPTable(1);
                    tableImage.HorizontalAlignment = Element.ALIGN_CENTER;
                    tableImage.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                    Image img = Image.GetInstance(GetImage());
                    img.ScaleAbsolute(100f, 100f);

                    tableImage.AddCell(img);

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
                    Paragraph name = new Paragraph("Nombre:", fontTitle);
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

                    Paragraph phName = new Paragraph("Yoandry", fontHeaderColumTwo);
                    Paragraph phAge = new Paragraph("36", fontHeaderColumTwo);
                    Paragraph phGender = new Paragraph("Masculino", fontHeaderColumTwo);
                    Paragraph phHeight = new Paragraph("175", fontHeaderColumTwo);
                    Paragraph phWeight = new Paragraph("86", fontHeaderColumTwo);
                    Paragraph phImc = new Paragraph("1.75", fontHeaderColumTwo);

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
                    AddChartToPage(args.PdfDoc);
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
                            cellData.Value = item.Type.ToString();
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
                         });

                    tips.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "1- Coma mucho dulce.";

                             properties.PdfFont = args.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                         });
                    tips.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "2- Beba mucha cerveza.";

                             properties.PdfFont = args.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                         });
                    tips.AddSimpleRow(
                         (cellData, properties) =>
                         {
                             cellData.Value = "3- No me carezca con la carne.";

                             properties.PdfFont = args.PdfFont;
                             properties.RunDirection = PdfRunDirection.LeftToRight;
                             properties.HorizontalAlignment = PdfRpt.Core.Contracts.HorizontalAlignment.Left;
                         });
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

        private byte[] GetChartsContent()
        {
            var data = GetData();
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
                     header.CacheHeader(cache: true);
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
                         AddChartToPage(args.PdfDoc);
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