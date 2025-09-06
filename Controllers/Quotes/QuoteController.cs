using MillenniumWebFixed.Models;
using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class QuoteController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // List all quotes
        public ActionResult Index()
        {
            var quotes = db.Quotes
                .OrderByDescending(q => q.CreatedAt)
                .ToList();

            return View(quotes);
        }

        // View details and versions of a quote
        public ActionResult Details(int id)
        {
            var quote = db.Quotes
                .Include("Versions.LineItems")
                .Include("Versions.Attachments")
                .FirstOrDefault(q => q.Id == id);

            if (quote == null)
                return HttpNotFound();

            return View(quote);
        }

        // Create new quote from project
        public ActionResult CreateFromProject(int projectId)
        {
            var project = db.GeneralQuoteDatas.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
                return HttpNotFound();

            // Create base quote
            var quote = new Quote
            {
                ProjectId = (int)project.Id,
                QuoteRef = $"Q-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}",
                CreatedBy = Session["FullName"]?.ToString() ?? "System",
                CreatedAt = DateTime.Now
            };

            db.Quotes.Add(quote);
            db.SaveChanges();

            // Add initial version
            var version = new QuoteVersion
            {
                QuoteId = quote.Id,
                VersionNumber = project.Version,
                Status = "Draft",
                CreatedBy = Session["FullName"]?.ToString() ?? "System",
                CreatedAt = DateTime.Now
            };

            db.QuoteVersions.Add(version);
            db.SaveChanges();

            // Now safe to generate line items
            var service = new QuoteBuilderService(db);
            int linesAdded = service.GenerateQuoteLineItems(version.Id, projectId);

            // Generate the PDF
            var pdfGenerator = new PdfTemplates.QuotePdfGenerator();
            byte[] pdfBytes = pdfGenerator.GeneratePdf(quote);

            // Save PDF to folder
            var fileName = $"{quote.QuoteRef}.pdf";
            var folderPath = Server.MapPath("~/PdfTemplates/GeneratedQuotes/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            // Save the path in the database
            quote.QuotePath = "/PdfTemplates/GeneratedQuotes/" + fileName;
            db.SaveChanges();

            TempData["Message"] = $"{linesAdded} line items generated. Quote PDF saved!";

            return RedirectToAction("Details", new { id = quote.Id });
        }

        protected string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public ActionResult DownloadQuote(int id)
        {
            var quote = db.Quotes
                          .Include("Versions.LineItems")
                          .FirstOrDefault(q => q.Id == id);
            if (quote == null)
                return HttpNotFound();

            // Generate the PDF
            var pdfGenerator = new PdfTemplates.QuotePdfGenerator();
            byte[] pdfBytes = pdfGenerator.GeneratePdf(quote);

            // Save PDF to folder
            var fileName = $"{quote.QuoteRef}.pdf";
            var folderPath = Server.MapPath("~/PdfTemplates/GeneratedQuotes/");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            var filePath = Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllBytes(filePath, pdfBytes);

            // Update the quote record
            quote.QuotePath = "/PdfTemplates/GeneratedQuotes/" + fileName;
            db.SaveChanges();

            // Redirect to the details page (optional)
            TempData["Message"] = "Quote saved successfully!";
            return RedirectToAction("Details", new { id = quote.Id });
        }
    }
}