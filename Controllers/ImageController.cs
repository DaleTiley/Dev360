using MillenniumWebFixed.ViewModels;
using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ImageController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // ------------------------------
        // PROJECT IMAGES (unchanged use)
        // ------------------------------

        public ActionResult ManageImages(int quoteId)
        {
            var quote = db.ManualQuotes.FirstOrDefault(q => q.Id == quoteId);
            if (quote == null) return HttpNotFound();

            var imgs = db.QuoteImages
                         .Where(x => x.QuoteId == quoteId)
                         .OrderByDescending(x => x.UploadedAt)
                         .ToList();

            //var imageCount = db.QuoteImages.Count(q => q.QuoteId == quote.Id);

            var model = new ManualQuoteViewModel
            {
                Id = quote.Id,
                QuoteNo = quote.QuoteNo,
                // Only keep this if you still need it elsewhere; it's not used for QuoteImages.
                // GeneralProjectDataId = quote.GeneralProjectDataId,
                QuoteImages = imgs
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult UploadImages(int id, IEnumerable<HttpPostedFileBase> imageFiles)
        {
            // LEGACY: Uploads into ProjectImages for a GeneralProjectData (not quotes)
            if (imageFiles != null)
            {
                string uploadPath = Server.MapPath("~/Uploads/ProjectImages");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                int fileCount = 0;

                foreach (var file in imageFiles)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        string extension = Path.GetExtension(file.FileName);
                        string uniqueName = $"{fileName}_{Guid.NewGuid():N}{extension}";
                        string filePath = Path.Combine(uploadPath, uniqueName);

                        file.SaveAs(filePath);

                        db.ProjectImages.Add(new ProjectImage
                        {
                            GeneralProjectDataId = id,
                            FileName = uniqueName,
                            FilePath = "/Uploads/ProjectImages/" + uniqueName,
                            UploadedAt = DateTime.Now
                        });

                        fileCount++;
                    }
                }

                db.SaveChanges();

                TempData[fileCount > 0 ? "Message" : "Error"] =
                    fileCount > 0 ? $"{fileCount} image(s) uploaded successfully."
                                  : "No valid files selected for upload.";
            }
            else
            {
                TempData["Error"] = "Please select at least one image.";
            }

            if (db.GeneralProjectDatas.Any(p => p.Id == id))
                return RedirectToAction("DetailsExcel", "Import", new { id });
            else
                return RedirectToAction("DetailsJson", "Import", new { id });
        }

        [HttpPost]
        public ActionResult DeleteImage(int id)
        {
            // LEGACY: Delete from ProjectImages
            var image = db.ProjectImages.Find(id);
            if (image == null) return HttpNotFound();

            int projectId = image.GeneralProjectDataId ?? 0;

            if (!string.IsNullOrEmpty(image.FilePath))
            {
                string physicalPath = Server.MapPath(image.FilePath);
                if (System.IO.File.Exists(physicalPath))
                    System.IO.File.Delete(physicalPath);
            }

            db.ProjectImages.Remove(image);
            db.SaveChanges();
            TempData["Message"] = "Image deleted successfully.";

            if (db.GeneralProjectDatas.Any(p => p.Id == projectId))
                return RedirectToAction("DetailsExcel", "Import", new { id = projectId });
            else
                return RedirectToAction("DetailsJson", "Import", new { id = projectId });
        }

        // --------------------------------
        // QUOTE IMAGES (new, for ManualQuote)
        // --------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadQuoteImages(int quoteId, IEnumerable<HttpPostedFileBase> files)
        {
            if (files == null || !files.Any())
                return Json(new { success = false, message = "No files received." });

            var uploadsRoot = Server.MapPath("~/Uploads/Quotes/" + quoteId);
            Directory.CreateDirectory(uploadsRoot);

            foreach (var file in files)
            {
                if (file == null || file.ContentLength == 0) continue;

                var fileName = Path.GetFileName(file.FileName);
                var uniqueName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
                var savePath = Path.Combine(uploadsRoot, uniqueName);
                file.SaveAs(savePath);

                db.QuoteImages.Add(new QuoteImage
                {
                    QuoteId = quoteId,
                    FileName = uniqueName,
                    FilePath = $"/Uploads/Quotes/{quoteId}/{uniqueName}",
                    UploadedAt = DateTime.UtcNow
                });
            }

            db.SaveChanges();

            var imgs = db.QuoteImages
                .Where(x => x.QuoteId == quoteId)
                .OrderByDescending(x => x.UploadedAt)
                .ToList();

            var html = RenderPartialViewToString("_QuoteImagesGrid", imgs);
            return Json(new { success = true, html });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQuoteImage(int id, int quoteId)
        {
            var img = db.QuoteImages.FirstOrDefault(x => x.Id == id && x.QuoteId == quoteId);
            if (img == null)
                return Json(new { success = false, message = "Not found." });

            if (!string.IsNullOrEmpty(img.FilePath))
            {
                var physical = Server.MapPath(img.FilePath);
                if (System.IO.File.Exists(physical)) System.IO.File.Delete(physical);
            }

            db.QuoteImages.Remove(img);
            db.SaveChanges();

            var imgs = db.QuoteImages
                .Where(x => x.QuoteId == quoteId)
                .OrderByDescending(x => x.UploadedAt)
                .ToList();

            var html = RenderPartialViewToString("_QuoteImagesGrid", imgs);
            return Json(new { success = true, html });
        }

        // Utility: render partial to string for AJAX responses
        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(
                    ControllerContext, viewResult.View, ViewData, TempData, sw
                );
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
