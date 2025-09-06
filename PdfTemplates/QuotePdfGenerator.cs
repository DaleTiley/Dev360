using iTextSharp.text;
using iTextSharp.text.pdf;
using MillenniumWebFixed.ViewModels;
using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace MillenniumWebFixed.PdfTemplates
{
    public class QuotePdfGenerator
    {
        public byte[] GeneratePdf(Quote quote)
        {
            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 50, 50, 80, 60);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                // Prepare header/footer details
                string logoPath = null;
                try
                {
                    logoPath = HttpContext.Current.Server.MapPath("~/PdfTemplates/Images/logo_white.png");
                }
                catch
                {
                    // Log or handle error gracefully
                }

                string contactDetails = "Millinnium Roofing | +27 12 377 3553 | info@mroofing.co.za";
                writer.PageEvent = new PdfHeaderFooter(quote.QuoteRef, contactDetails, logoPath);

                document.Open();

                // Add Title
                var titleFont = FontFactory.GetFont("Arial", 16, Font.BOLD);
                document.Add(new Paragraph($"Quote: {quote.QuoteRef}", titleFont));
                document.Add(new Paragraph($"Project ID: {quote.ProjectId}"));
                document.Add(new Paragraph($"Date: {quote.CreatedAt:yyyy-MM-dd}"));

                document.Add(new Paragraph(" ")); // spacing

                // Add line items table
                // Add line items table
                var table = new PdfPTable(5);
                table.WidthPercentage = 100;

                // Define header font and background
                var headerFont = FontFactory.GetFont("Arial", 9, Font.BOLD, BaseColor.WHITE);
                var headerBackground = new BaseColor(50, 50, 50); // dark gray

                // Define body font
                var bodyFont = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK);

                // Add header cells with styling
                string[] headers = { "Product", "Description", "Quantity", "Unit", "Unit Price" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, headerFont))
                    {
                        BackgroundColor = headerBackground,
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        Padding = 5
                    };
                    table.AddCell(cell);
                }

                // Add data rows
                bool isOdd = true;
                foreach (var version in quote.Versions)
                {
                    foreach (var item in version.LineItems)
                    {
                        var backgroundColor = isOdd ? BaseColor.WHITE : new BaseColor(240, 240, 240); // light gray
                        isOdd = !isOdd;

                        var productCell = new PdfPCell(new Phrase(item.JsonItemName ?? string.Empty, bodyFont))
                        {
                            BackgroundColor = backgroundColor,
                            Padding = 4,
                            BorderWidth = 0.5f
                        };

                        var descCell = new PdfPCell(new Phrase(item.ProductDesc ?? string.Empty, bodyFont))
                        {
                            BackgroundColor = backgroundColor,
                            Padding = 4,
                            BorderWidth = 0.5f
                        };

                        var qtyCell = new PdfPCell(new Phrase(item.Quantity.ToString(), bodyFont))
                        {
                            BackgroundColor = backgroundColor,
                            Padding = 4,
                            BorderWidth = 0.5f,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };

                        var unitCell = new PdfPCell(new Phrase(item.Unit ?? string.Empty, bodyFont))
                        {
                            BackgroundColor = backgroundColor,
                            Padding = 4,
                            BorderWidth = 0.5f
                        };

                        var priceCell = new PdfPCell(new Phrase(item.UnitSell.HasValue
                            ? string.Format("{0:C}", item.UnitSell.Value)
                            : "N/A", bodyFont))
                        {
                            BackgroundColor = backgroundColor,
                            Padding = 4,
                            BorderWidth = 0.5f,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };

                        table.AddCell(productCell);
                        table.AddCell(descCell);
                        table.AddCell(qtyCell);
                        table.AddCell(unitCell);
                        table.AddCell(priceCell);
                    }
                }



                document.Add(table);

                document.Close();

                return memoryStream.ToArray();
            }
        }

        public string GenerateManualQuotePdf(int quoteId, string outputDir, string quoteNumber, string revisionID)
        {
            var filename = $"{quoteNumber}{revisionID}_{DateTime.Now:yyyyMMddHHmm}.pdf";
            var fullPath = Path.Combine(outputDir, filename);

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var db = new AppDbContext())
            {
                var quote = db.ManualQuotes
                    .Include("LineItems.Product")
                    .Include("PotentialCustomer")
                    .Include("SalesRep")
                    .Include("Contact")
                    .Include("Enquiry")
                    .Include("Designer")
                    .FirstOrDefault(q => q.Id == quoteId);

                if (quote == null)
                    throw new Exception("Quote not found.");

                var model = new ManualQuoteViewModel
                {
                    QuoteNo = quote.QuoteNo,
                    RevisionID = quote.RevisionID,
                    ImportDate = quote.ImportDate,
                    PotentialCustomerName = quote.PotentialCustomer?.FullName,
                    SelectedSalesRepName = quote.SalesRep?.FullName,
                    SelectedContactName = quote.Contact?.Name,
                    SelectedEnquiryNumber = quote.Enquiry?.EnquiryNumber,
                    SiteName = quote.SiteName,
                    StreetAddress = quote.StreetAddress,
                    StandNumber = quote.StandNumber,
                    ProjectName = quote.ProjectName,
                    UnitBlockNumber = quote.UnitBlockNumber,
                    UnitBlockType = quote.UnitBlockType,
                    RoofPitch = quote.RoofPitch,
                    RoofOverhang = quote.RoofOverhang,
                    RoofGableOverhang = quote.RoofGableOverhang,
                    RoofCovering = quote.RoofCovering,
                    MaxBattenCenters = quote.MaxBattenCenters,
                    MaxTrustCenters = quote.MaxTrustCenters,
                    FloorArea = quote.FloorArea,
                    RoofArea = quote.RoofArea,
                    SelectedDesignerName = quote.Designer?.FullName,
                    DueDate = quote.DueDate,
                    ImportedPMRJBFileRef = quote.ImportedPMRJBFileRef,
                    QuoteNotes = quote.QuoteNotes,
                    SalesNotes = quote.SalesNotes,
                    ShipToStreet1 = quote.ShipToStreet1,
                    ShipToStreet2 = quote.ShipToStreet2,
                    ShipToStreet3 = quote.ShipToStreet3,
                    ShipToCity = quote.ShipToCity,
                    ShipToState = quote.ShipToState,
                    ShipToPostalCode = quote.ShipToPostalCode,
                    ShipToCountry = quote.ShipToCountry,
                    LineItems = quote.LineItems.Select(li => new QuoteLineItemViewModel
                    {
                        ProductId = li.ProductId,
                        ProductCode = li.Product?.ProductCode,
                        ProductDescription = li.Product?.Name,
                        BaseUOM = li.Product?.BaseUOM,
                        Qty = li.Qty,
                        CostPrice = li.CostPrice,
                        MarginPercent = li.MarginPercent,
                        SellingPrice = li.SellingPrice
                    }).ToList()
                };

                var document = new Document(PageSize.A4, 50, 50, 60, 60);
                var writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Fonts & colors
                var fontTitle = FontFactory.GetFont("Arial", 14, Font.BOLD);
                var fontBody = FontFactory.GetFont("Arial", 8);
                var fontBold = FontFactory.GetFont("Arial", 8, Font.BOLD);
                var sectionTitleFont = FontFactory.GetFont("Arial", 10, Font.BOLD);
                var grandFont = FontFactory.GetFont("Arial", 10, Font.BOLD, new BaseColor(200, 0, 0));
                var headerBg = new BaseColor(230, 230, 230);
                var shadeBg = new BaseColor(245, 245, 245);
                var red = new BaseColor(200, 0, 0);

                // Logo
                try
                {
                    var logoPath = HttpContext.Current.Server.MapPath("~/PdfTemplates/Images/logo_white.png");
                    var logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(100f, 35f);
                    logo.Alignment = Element.ALIGN_RIGHT;
                    document.Add(logo);
                }
                catch { }

                // Title (Quote Number)
                document.Add(new Paragraph($"Manual Quote: {model.QuoteNo}", fontTitle)
                {
                    SpacingAfter = 4f
                });

                // Subheading (Revision)
                //var revisionFont = FontFactory.GetFont("Arial", 10, Font.ITALIC, BaseColor.DARK_GRAY);
                document.Add(new Paragraph($"Revision: {model.RevisionID}", fontTitle)
                {
                    SpacingAfter = 12f
                });


                void AddSectionTitle(string text)
                {
                    document.Add(new Paragraph(text, sectionTitleFont)
                    {
                        SpacingBefore = 10f,
                        SpacingAfter = 5f
                    });
                }

                void AddKeyValueTable(Dictionary<string, string> data)
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 30, 70 });

                    foreach (var kv in data)
                    {
                        table.AddCell(new PdfPCell(new Phrase(kv.Key, fontBold)) { Border = 0 });
                        table.AddCell(new PdfPCell(new Phrase(kv.Value ?? "", fontBody)) { Border = 0 });
                    }

                    document.Add(table);
                }

                // Info Sections
                AddSectionTitle("Quote Information");
                AddKeyValueTable(new Dictionary<string, string>
        {
            { "Quote Date:", model.ImportDate?.ToString("yyyy-MM-dd") },
            { "Customer:", model.PotentialCustomerName },
            { "Site Name:", model.SiteName },
            { "Sales Person:", model.SelectedSalesRepName },
            { "Contact Person:", model.SelectedContactName },
            { "Enquiry Ref:", model.SelectedEnquiryNumber }
        });

                AddSectionTitle("Site Details");
                AddKeyValueTable(new Dictionary<string, string>
        {
            { "Stand No:", model.StandNumber },
            { "Unit/Block No:", model.UnitBlockNumber },
            { "Type:", model.UnitBlockType }
        });

                AddSectionTitle("Design Details");

                var designTable = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 5f,
                    SpacingAfter = 10f
                };
                designTable.SetWidths(new float[] { 25, 25, 25, 25 });

                void AddDesignRow(string label1, string value1, string label2, string value2)
                {
                    PdfPCell CreateCell(string label, string value)
                    {
                        var p = new Paragraph();
                        p.Add(new Chunk(label, fontBold));
                        p.Add(new Chunk(" " + (value ?? ""), fontBody));

                        return new PdfPCell(p)
                        {
                            Border = Rectangle.NO_BORDER,
                            Padding = 5f
                        };
                    }

                    designTable.AddCell(CreateCell(label1, value1));
                    designTable.AddCell(CreateCell(label2, value2));
                }

                // Add all rows (clean, no background)
                AddDesignRow("Roof Pitch:", model.RoofPitch, "Max Batten Centers:", model.MaxBattenCenters);
                AddDesignRow("Roof Overhang:", model.RoofOverhang, "Max Truss Centers:", model.MaxTrustCenters);
                AddDesignRow("Roof Gable Overhang:", model.RoofGableOverhang, "Floor Area:", model.FloorArea);
                AddDesignRow("Roof Covering:", model.RoofCovering, "Roof Area:", model.RoofArea);

                document.Add(designTable);



                AddSectionTitle("Sales / Design Info");
                AddKeyValueTable(new Dictionary<string, string>
                    {
                        { "Sales Rep:", model.SelectedSalesRepName },
                        { "Designer:", model.SelectedDesignerName },
                        { "Due Date:", model.DueDate?.ToString("yyyy-MM-dd") },
                        { "PMRJB File Ref:", model.ImportedPMRJBFileRef },
                        { "Quote Notes:", model.QuoteNotes },
                        { "Sales Notes:", model.SalesNotes }
                    });

                AddSectionTitle("Shipping Address");
                if (!string.IsNullOrWhiteSpace(model.ShipToStreet1))
                    document.Add(new Paragraph(model.ShipToStreet1, fontBody));

                // Line Items
                AddSectionTitle("Line Items");

                var itemTable = new PdfPTable(6)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 10f
                };
                // Adjusted widths: Code, Description, Qty, UOM, Unit Price, Line Total
                itemTable.SetWidths(new float[] { 10, 44, 8, 8, 15, 15 });

                void AddCell(string text, Font font, int align = Element.ALIGN_LEFT, BaseColor bg = null)
                {
                    var cell = new PdfPCell(new Phrase(text, font))
                    {
                        HorizontalAlignment = align,
                        Padding = 4f,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    };
                    if (bg != null) cell.BackgroundColor = bg;
                    itemTable.AddCell(cell);
                }

                // Header row
                AddCell("Code", fontBold, Element.ALIGN_LEFT, headerBg);
                AddCell("Description", fontBold, Element.ALIGN_LEFT, headerBg);
                AddCell("Qty", fontBold, Element.ALIGN_CENTER, headerBg);
                AddCell("UOM", fontBold, Element.ALIGN_CENTER, headerBg);
                AddCell("Unit Price", fontBold, Element.ALIGN_RIGHT, headerBg);
                AddCell("Line Total", fontBold, Element.ALIGN_RIGHT, headerBg);

                bool shade = false;
                foreach (var item in model.LineItems)
                {
                    var bg = shade ? shadeBg : null;
                    shade = !shade;

                    AddCell(item.ProductCode ?? "", fontBody, Element.ALIGN_LEFT, bg);
                    AddCell(item.ProductDescription ?? "", fontBody, Element.ALIGN_LEFT, bg);
                    AddCell(item.Qty.ToString("0.##"), fontBody, Element.ALIGN_CENTER, bg);
                    AddCell(item.BaseUOM ?? "", fontBody, Element.ALIGN_CENTER, bg);
                    AddCell(item.SellingPrice?.ToString("C") ?? "R 0.00", fontBody, Element.ALIGN_RIGHT, bg);
                    AddCell((item.SellingPrice.GetValueOrDefault() * item.Qty).ToString("C"), fontBody, Element.ALIGN_RIGHT, bg);
                }

                document.Add(itemTable);


                // Totals
                decimal subtotal = model.LineItems.Sum(i => i.SellingPrice.GetValueOrDefault() * i.Qty);
                decimal vat = subtotal * 0.15m;
                decimal grandTotal = subtotal + vat;

                var totalsTable = new PdfPTable(2)
                {
                    WidthPercentage = 40,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 10f
                };
                totalsTable.SetWidths(new float[] { 60, 40 });

                // Subtotal and VAT - regular formatting
                totalsTable.AddCell(new PdfPCell(new Phrase("Subtotal:", fontBody))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 2f
                });
                totalsTable.AddCell(new PdfPCell(new Phrase(subtotal.ToString("C"), fontBody))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 2f
                });

                totalsTable.AddCell(new PdfPCell(new Phrase("VAT (15%):", fontBody))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 2f
                });
                totalsTable.AddCell(new PdfPCell(new Phrase(vat.ToString("C"), fontBody))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 2f
                });

                // Grand Total - bold, double (thick) top border
                var grandTotalLabelCell = new PdfPCell(new Phrase("Grand Total:", fontBold))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 1.5f,
                    BorderColorTop = BaseColor.BLACK,
                    PaddingTop = 6f,
                    PaddingBottom = 6f,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };

                var grandTotalValueCell = new PdfPCell(new Phrase(grandTotal.ToString("C"), fontBold))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderWidthTop = 1.5f,
                    BorderColorTop = BaseColor.BLACK,
                    PaddingTop = 6f,
                    PaddingBottom = 6f,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                };

                totalsTable.AddCell(grandTotalLabelCell);
                totalsTable.AddCell(grandTotalValueCell);

                document.Add(totalsTable);
                document.Close();

            }

            return fullPath;
        }
    }
}
