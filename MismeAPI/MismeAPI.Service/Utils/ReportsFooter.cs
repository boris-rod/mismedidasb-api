using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfRpt.Core.Contracts;
using System.Collections.Generic;

namespace MismeAPI.Service.Utils
{
    public class ReportsFooter : IPageFooter
    {
        private PdfContentByte _pdfContentByte;
        private readonly IPdfFont _pdfRptFont;
        private readonly iTextSharp.text.Font _font;
        private readonly PdfRunDirection _direction;
        private PdfTemplate _template;

        public ReportsFooter(IPdfFont pdfRptFont, PdfRunDirection direction)
        {
            _direction = direction;
            _pdfRptFont = pdfRptFont;
            _font = _pdfRptFont.Fonts[0];
        }

        public void ClosingDocument(PdfWriter writer, Document document, IList<SummaryCellData> columnCellsSummaryData)
        {
            _template.BeginText();
            _template.SetFontAndSize(_pdfRptFont.Fonts[0].BaseFont, 8);
            _template.SetTextMatrix(0, 0);
            _template.ShowText((writer.PageNumber - 1).ToString());
            _template.EndText();
        }

        public void PageFinished(PdfWriter writer, Document document, IList<SummaryCellData> columnCellsSummaryData)
        {
            var pageSize = document.PageSize;
            var text = "Dra. Saira R. Rivas Suárez - Este informe no constituye un documento clínico";
            var textLen = _font.BaseFont.GetWidthPoint(text, _font.Size);
            var center = (pageSize.Left + pageSize.Right) / 2;
            var align = Element.ALIGN_CENTER;

            ColumnText.ShowTextAligned(
                        canvas: _pdfContentByte,
                        alignment: align,
                        phrase: new Phrase(text, _font),
                        x: center,
                        y: pageSize.GetBottom(25),
                        rotation: 0,
                        runDirection: (int)_direction,
                        arabicOptions: 0);

            var x = _direction == PdfRunDirection.RightToLeft ? center - textLen : center + textLen;
            _pdfContentByte.AddTemplate(_template, x, pageSize.GetBottom(25));
        }

        public void DocumentOpened(PdfWriter writer, IList<SummaryCellData> columnCellsSummaryData)
        {
            _pdfContentByte = writer.DirectContent;
            _template = _pdfContentByte.CreateTemplate(50, 50);
        }
    }
}