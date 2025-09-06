using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace MillenniumWebFixed.PdfTemplates
{
    public class PdfHeaderFooter : PdfPageEventHelper
    {
        private readonly string _quoteNumber;
        private readonly string _contactDetails;
        private readonly string _logoPath;

        public PdfHeaderFooter(string quoteNumber, string contactDetails, string logoPath)
        {
            _quoteNumber = quoteNumber;
            _contactDetails = contactDetails;
            _logoPath = logoPath;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            try
            {
                PdfContentByte cb = writer.DirectContent;

                // Draw header table
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                headerTable.DefaultCell.Border = 0;

                // Logo cell
                if (!string.IsNullOrWhiteSpace(_logoPath) && File.Exists(_logoPath))
                {
                    try
                    {
                        var logo = Image.GetInstance(_logoPath);
                        logo.ScaleToFit(50f, 50f);
                        PdfPCell logoCell = new PdfPCell(logo)
                        {
                            Border = 0,
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            PaddingBottom = 5f
                        };
                        headerTable.AddCell(logoCell);
                    }
                    catch
                    {
                        headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });
                    }
                }
                else
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });
                }

                // Quote number cell
                string quoteText = !string.IsNullOrWhiteSpace(_quoteNumber)
                    ? $"Quote: {_quoteNumber}"
                    : "Quote";
                PdfPCell titleCell = new PdfPCell(new Phrase(quoteText, FontFactory.GetFont("Arial", 12, Font.BOLD)))
                {
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingBottom = 5f
                };
                headerTable.AddCell(titleCell);

                headerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.PageSize.Height - 10, cb);

                // Draw horizontal line below header
                cb.MoveTo(document.LeftMargin, document.PageSize.Height - 40);
                cb.LineTo(document.PageSize.Width - document.RightMargin, document.PageSize.Height - 40);
                cb.Stroke();

                // Footer table
                PdfPTable footerTable = new PdfPTable(2);
                footerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
                footerTable.DefaultCell.Border = 0;

                string contactText = !string.IsNullOrWhiteSpace(_contactDetails)
                    ? _contactDetails
                    : "Company Name | 000-000-0000 | email@example.com";
                PdfPCell contactCell = new PdfPCell(new Phrase(contactText, FontFactory.GetFont("Arial", 8, Font.ITALIC)))
                {
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                footerTable.AddCell(contactCell);

                string pageText = $"Page {writer.PageNumber}";
                PdfPCell pageCell = new PdfPCell(new Phrase(pageText, FontFactory.GetFont("Arial", 8, Font.ITALIC)))
                {
                    Border = 0,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };
                footerTable.AddCell(pageCell);

                footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, cb);

                // Draw horizontal line above footer
                cb.MoveTo(document.LeftMargin, document.BottomMargin);
                cb.LineTo(document.PageSize.Width - document.RightMargin, document.BottomMargin);
                cb.Stroke();
            }
            catch
            {
                // Log error or ignore gracefully
            }
        }
    }
}
