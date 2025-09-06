using MillenniumWebFixed.Helpers;
using MillenniumWebFixed.Models;
using MillenniumWebFixed.Security;
using MillenniumWebFixed.Services; // AuditLogger, AuditArea
using MillenniumWebFixed.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();


        // -------- Project Quotes ----
        // ProjectsController.cs
        [HttpGet]
        public PartialViewResult ProjectQuotes(int id)
        {
            ViewBag.ProjectId = id;

            // Pull quotes for this project. (Requires ProjectId in ProjectQuotes)
            var tmp = db.ProjectQuotes
                .Where(q => q.ProjectId == id)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new
                {
                    q.QuoteId,
                    q.QuoteNumber,
                    q.BuildingType,
                    q.QuotationOption,
                    q.Revision,
                    q.CreatedAt,
                    q.Notes,
                    q.Status,
                    q.TotalAmount,
                    q.CreatedByUserId
                })
                .ToList(); // closes the reader

            var rows = tmp.Select(x => new ProjectQuoteRowVm
            {
                Id = x.QuoteId,
                QuoteNo = x.QuoteNumber,
                Building = x.BuildingType,
                Variant = x.QuotationOption.ToString(),
                Revision = x.Revision,
                Date = x.CreatedAt,
                Description = x.Notes,
                Status = x.Status,
                TotalAmount = x.TotalAmount,
                CreatedBy = GetUserDisplayName(x.CreatedByUserId ?? 0) // now safe (but N+1)
            }).ToList();

            return PartialView("~/Views/Projects/Partials/_ProjectQuotes.cshtml", rows);
        }

        [HttpGet]
        public ActionResult QuickSummary(int id)
        {
            var p = db.Projects.FirstOrDefault(x => x.Id == id);
            if (p == null) return HttpNotFound();

            // Counts (adjust table names if yours differ)
            var quotesCount = db.ProjectQuotes.Count(q => q.ProjectId == id);

            // If you don’t have a Tenders table yet, leave 0 or add the real query later.
            //var tendersCount = db.Tenders.Any()
            //    ? db.Tenders.Count(t => t.ProjectId == id)
            //    : 0;

            var attachmentsCount = db.ProjectDocuments.Count(d => d.ProjectId == id);

            var vm = new ProjectQuickSummaryVm
            {
                Id = p.Id,
                ProjectCode = p.ProjectCode,
                //ProjectName = p.ProjectName,              // if you don’t have this, it’s ok to leave null
                ClientName = p.ClientName,
                ContactName = p.ContactName,
                SiteAddress1 = p.SiteAddress1,
                SiteCity = p.SiteCity,
                SiteProvince = p.SiteProvince,
                SitePostcode = p.SitePostcode,
                Status = p.Status,
                Designer = p.Designer,
                QuotesCount = quotesCount,
                TendersCount = 0, //tendersCount,
                AttachmentsCount = attachmentsCount,
                CreatedBy = p.CreatedBy,                  // or p.CreatedByName if that’s what you store
                CreatedAtDisplay = p.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                ERPNumber = p.ERPNumber,
                SalesPerson = p.SalesPerson,
                Township = p.Township,
                StandNumber = p.StandNumber,
                PortionNumber = p.PortionNumber
            };

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        // -------- utils --------
        private int GetCurrentUserId()
        {
            return User is CustomPrincipal me ? me.Id : 0;
        }

        private string GetUserDisplayName(int userId)
        {
            var u = db.AppUsers.AsNoTracking().FirstOrDefault(x => x.Id == userId);
            return u?.FullName ?? $"User #{userId}";
        }

        // -------- list --------
        public ActionResult Index()
        {
            var items = db.Projects
                          .OrderByDescending(p => p.Id)
                          .Take(100)
                          .ToList();
            return View(items);
        }

        // -------- audit tab (joins AppUsers for names) --------
        [HttpGet]
        public PartialViewResult AuditTab(int id)
        {
            var items = (from a in db.ProjectAudits.AsNoTracking()
                         where a.ProjectId == id
                         orderby a.ChangedAt descending
                         select new ProjectAuditListItem
                         {
                             ChangedAt = a.ChangedAt,
                             Area = a.Area,
                             Description = a.Description,
                             ChangedByUserId = a.ChangedByUserId,
                             ChangedByName = db.AppUsers
                                 .Where(u => u.Id == a.ChangedByUserId)
                                 .Select(u => u.FullName)
                                 .FirstOrDefault()
                         })
                        .Take(500)
                        .ToList();

            // fill missing names gracefully
            items.ForEach(i => { if (string.IsNullOrWhiteSpace(i.ChangedByName)) i.ChangedByName = $"User #{i.ChangedByUserId}"; });

            return PartialView("Partials/_ProjectAudit", items);
        }

        // -------- create --------
        public ActionResult Create()
        {
            var vm = new ProjectCreateVm();
            vm.SalesRepOptions = BuildSalesRepOptions(null);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SetStatus(int id, string status)
        {
            var prj = db.Projects.FirstOrDefault(p => p.Id == id);
            if (prj == null) return HttpNotFound();

            status = (status ?? "").Trim();
            if (string.IsNullOrEmpty(status))
            {
                TempData["ToastMessage"] = "Please choose a valid status.";
                return RedirectToAction("Details", new { id });
            }

            // snapshot BEFORE
            var before = new { prj.Status };

            // update
            prj.Status = status;
            db.SaveChanges();

            // audit (will render "Status: 'Old' → 'New'")
            var me = User as CustomPrincipal;
            AuditLogger.LogFieldChanges(
                db, prj.Id, me.Id, AuditArea.General,
                before,
                new { prj.Status });

            TempData["ToastMessage"] = $"Project status set to {status}.";
            return RedirectToAction("Details", new { id });
        }

        [HttpGet]
        public async Task<JsonResult> CheckProjectCode(string code, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Json(new { ok = false, reason = "empty" }, JsonRequestBehavior.AllowGet);

            code = code.Trim();
            var exists = await db.Projects
                .AsNoTracking()
                .AnyAsync(p => p.ProjectCode == code && (!excludeId.HasValue || p.Id != excludeId.Value));

            return Json(new { ok = !exists }, JsonRequestBehavior.AllowGet);
        }

        private string GetNextProjectCodeFromDb()
        {
            using (var cmd = db.Database.Connection.CreateCommand())
            {
                cmd.CommandText = "EXEC dbo.AllocateNextProjectCode";
                if (cmd.Connection.State != System.Data.ConnectionState.Open)
                    cmd.Connection.Open();
                return Convert.ToString(cmd.ExecuteScalar());
            }
        }

        private const string CASH_SALE_NAME = "Cash Sale";

        private string EnsureClientNameOrDefaultCashSale(string clientName)
        {
            var name = (clientName ?? "").Trim();

            if (!string.IsNullOrEmpty(name))
                return name; // keep whatever was provided

            // Ensure a "Cash Sale" customer exists (no contacts)
            var cash = db.Customers.FirstOrDefault(c => c.CustomerName == CASH_SALE_NAME);
            if (cash == null)
            {
                cash = new Customer
                {
                    CustomerName = CASH_SALE_NAME,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                db.Customers.Add(cash);
                db.SaveChanges();
            }

            return CASH_SALE_NAME;
        }

        // POST: /Projects/Create
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(ProjectCreateVm vm)
        {
            // 1) Generate code server-side
            if (string.IsNullOrWhiteSpace(vm.ProjectCode))
            {
                vm.ProjectCode = GetNextProjectCodeFromDb();
                ModelState.Remove(nameof(vm.ProjectCode)); // remove posted empty value
            }

            // 2) Default client name to Cash Sale when blank
            var originalClientName = vm.ClientName; // might be empty from post
            vm.ClientName = EnsureClientNameOrDefaultCashSale(vm.ClientName);
            if (string.IsNullOrWhiteSpace(originalClientName))
            {
                ModelState.Remove(nameof(vm.ClientName));
                ModelState.Remove(nameof(vm.ClientId));
            }

            if (!ModelState.IsValid) return View(vm);

            // Resolve current user name (use what you already have in _Layout)
            var rawName = User?.Identity?.IsAuthenticated == true
                ? (string.IsNullOrWhiteSpace(User.GetFullName())
                    ? User.Identity.Name
                    : User.GetFullName())
                : (Session["user"] as string);

            var createdBy = (rawName ?? "System").Trim();

            vm.CreatedAt = DateTime.UtcNow;         // keep UTC; you format in views
            vm.CreatedBy = createdBy;
            vm.CreatedByUserId = User.GetUserId();

            // Basic duplicate guard on ProjectCode
            var exists = db.Projects.Any(p => p.ProjectCode == vm.ProjectCode);
            if (exists)
            {
                ModelState.AddModelError("ProjectCode", "A project with this code already exists.");
                return View(vm);
            }

            // Map SalesRepId -> name string for storage
            var selectedSalesName = FindSalesRepNameById(vm.SalesRepId);

            var prj = new Project
            {
                // Core
                ProjectCode = vm.ProjectCode?.Trim(),
                ClientName = vm.ClientName?.Trim(),
                SiteName = vm.SiteName?.Trim(),
                CreatedAt = DateTime.UtcNow,
                Status = "Active",

                // Contact
                ContactName = vm.ContactName?.Trim(),
                ContactEmail = vm.ContactEmail?.Trim(),
                ContactPhone = vm.ContactPhone?.Trim(),

                // Address
                SiteAddress1 = vm.SiteAddress1?.Trim(),
                SiteAddress2 = vm.SiteAddress2?.Trim(),
                SiteCity = vm.SiteCity?.Trim(),
                SitePostcode = vm.SitePostcode?.Trim(),
                SiteProvince = vm.SiteProvince?.Trim(),

                // Notes
                Notes = vm.Notes,

                // Legal/Location/Links
                ERPNumber = vm.ERPNumber?.Trim(),
                Township = vm.Township?.Trim(),
                PortionNumber = vm.PortionNumber?.Trim(),
                StandNumber = vm.StandNumber?.Trim(),
                StreetAddress = vm.StreetAddress?.Trim(),
                GoogleMapUrl = vm.GoogleMapUrl?.Trim(),
                SharePointUrl = vm.SharePointUrl?.Trim(),
                SiteRentals = vm.SiteRentals?.Trim(),

                // People
                SalesPerson = selectedSalesName?.Trim(),
                SiteContactName = vm.SiteContactName?.Trim(),
                SiteContactPhone = vm.SiteContactPhone?.Trim(),

                // CRM-lite
                CrmStage = vm.CrmStage?.Trim(),
                CrmNextAction = vm.CrmNextAction?.Trim(),
                CrmFollowUpDate = vm.CrmFollowUpDate
            };

            db.Projects.Add(prj);
            db.SaveChanges(); // saved so prj.Id exists  :contentReference[oaicite:2]{index=2}

            // Audit: project created
            var me = User as CustomPrincipal;
            AuditLogger.Log(
                db,
                projectId: prj.Id,
                userId: me.Id,
                area: AuditArea.General,
                description: $"Project created (Code: {prj.ProjectCode}, Client: {prj.ClientName})",
                meta: new { prj.ProjectCode, prj.ClientName });

            // Save files (if any) — also writes an audit row if anything uploaded
            SaveProjectFiles(prj.Id, vm.Files);

            TempData["Message"] = "Project created.";
            return RedirectToAction("Details", new { id = prj.Id });
        }

        // -------- details --------
        public ActionResult Details(int id)
        {
            var prj = db.Projects.FirstOrDefault(p => p.Id == id);
            if (prj == null) return HttpNotFound();

            var docs = db.ProjectDocuments.Where(d => d.ProjectId == id)
                                          .OrderByDescending(d => d.UploadedAt)
                                          .ToList();
            ViewBag.Documents = docs;

            return View(prj);
        }

        // -------- edit --------
        public ActionResult Edit(int id)
        {
            var prj = db.Projects.FirstOrDefault(p => p.Id == id);
            if (prj == null) return HttpNotFound();

            var vm = new ProjectCreateVm
            {
                // Core
                ProjectCode = prj.ProjectCode,
                ClientName = prj.ClientName,
                SiteName = prj.SiteName,

                // Contact
                ContactName = prj.ContactName,
                ContactEmail = prj.ContactEmail,
                ContactPhone = prj.ContactPhone,

                // Address
                SiteAddress1 = prj.SiteAddress1,
                SiteAddress2 = prj.SiteAddress2,
                SiteCity = prj.SiteCity,
                SitePostcode = prj.SitePostcode,
                SiteProvince = prj.SiteProvince,

                // Notes
                Notes = prj.Notes,

                // Legal/Location/Links
                ERPNumber = prj.ERPNumber,
                Township = prj.Township,
                PortionNumber = prj.PortionNumber,
                StandNumber = prj.StandNumber,
                StreetAddress = prj.StreetAddress,
                GoogleMapUrl = prj.GoogleMapUrl,
                SharePointUrl = prj.SharePointUrl,
                SiteRentals = prj.SiteRentals,

                // People
                SalesPerson = prj.SalesPerson,
                SiteContactName = prj.SiteContactName,
                SiteContactPhone = prj.SiteContactPhone,

                // CRM-lite
                CrmStage = prj.CrmStage,
                CrmNextAction = prj.CrmNextAction,
                CrmFollowUpDate = prj.CrmFollowUpDate
            };

            vm.SalesRepId = FindSalesRepIdByName(prj.SalesPerson);
            vm.SalesRepOptions = BuildSalesRepOptions(vm.SalesRepId);

            ViewBag.ProjectId = prj.Id;
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ProjectCreateVm vm)
        {
            var prj = db.Projects.FirstOrDefault(p => p.Id == id);
            if (prj == null) return HttpNotFound();

            // Edit does not require these IDs (we store names on the project)
            ModelState.Remove(nameof(vm.ClientId));
            ModelState.Remove(nameof(vm.ContactId));

            // Avoid stale value overriding dropdown selection on redisplay
            ModelState.Remove(nameof(vm.SalesRepId));

            if (!ModelState.IsValid)
            {
                vm.SalesRepOptions = BuildSalesRepOptions(vm.SalesRepId);
                ViewBag.ProjectId = id;
                return View(vm);
            }

            // ----- BEFORE snapshot (take this BEFORE making changes) -----
            var before = new
            {
                prj.ClientName,
                prj.ContactName,
                prj.ContactEmail,
                prj.ContactPhone,
                prj.SiteName,
                prj.StreetAddress,
                prj.SiteAddress1,
                prj.SiteAddress2,
                prj.SiteCity,
                prj.SiteProvince,
                prj.SitePostcode,
                prj.ERPNumber,
                prj.Township,
                prj.PortionNumber,
                prj.StandNumber,
                prj.SalesPerson,
                prj.SiteContactName,
                prj.SiteContactPhone,
                prj.GoogleMapUrl,
                prj.SharePointUrl,
                prj.SiteRentals,
                prj.CrmStage,
                prj.CrmNextAction,
                prj.CrmFollowUpDate,
                prj.Notes
            };

            // Default client to Cash Sale if blank (see section 3)
            vm.ClientName = EnsureClientNameOrDefaultCashSale(vm.ClientName);

            // ----- map fields from VM -----
            // Core
            prj.ProjectCode = vm.ProjectCode?.Trim();
            prj.ClientName = vm.ClientName?.Trim();
            prj.SiteName = vm.SiteName?.Trim();

            // Contact
            prj.ContactName = vm.ContactName?.Trim();
            prj.ContactEmail = vm.ContactEmail?.Trim();
            prj.ContactPhone = vm.ContactPhone?.Trim();

            // Address
            prj.SiteAddress1 = vm.SiteAddress1?.Trim();
            prj.SiteAddress2 = vm.SiteAddress2?.Trim();
            prj.SiteCity = vm.SiteCity?.Trim();
            prj.SitePostcode = vm.SitePostcode?.Trim();
            prj.SiteProvince = vm.SiteProvince?.Trim();

            // Notes
            prj.Notes = vm.Notes;

            // Legal / Location / Links
            prj.ERPNumber = vm.ERPNumber?.Trim();
            prj.Township = vm.Township?.Trim();
            prj.PortionNumber = vm.PortionNumber?.Trim();
            prj.StandNumber = vm.StandNumber?.Trim();
            prj.StreetAddress = vm.StreetAddress?.Trim();
            prj.GoogleMapUrl = vm.GoogleMapUrl?.Trim();
            prj.SharePointUrl = vm.SharePointUrl?.Trim();
            prj.SiteRentals = vm.SiteRentals?.Trim();

            // People
            // Map SalesRepId -> name; only overwrite if a rep was chosen
            var chosenSalesName = FindSalesRepNameById(vm.SalesRepId);
            if (!string.IsNullOrWhiteSpace(chosenSalesName))
                prj.SalesPerson = chosenSalesName;

            prj.SiteContactName = vm.SiteContactName?.Trim();
            prj.SiteContactPhone = vm.SiteContactPhone?.Trim();

            // CRM-lite
            prj.CrmStage = vm.CrmStage?.Trim();
            prj.CrmNextAction = vm.CrmNextAction?.Trim();
            prj.CrmFollowUpDate = vm.CrmFollowUpDate;

            db.SaveChanges();

            // ----- audit diff -----
            var me = User as CustomPrincipal;
            AuditLogger.LogFieldChanges(
                db, prj.Id, me.Id, AuditArea.General, before, new
                {
                    prj.ClientName,
                    prj.ContactName,
                    prj.ContactEmail,
                    prj.ContactPhone,
                    prj.SiteName,
                    prj.StreetAddress,
                    prj.SiteAddress1,
                    prj.SiteAddress2,
                    prj.SiteCity,
                    prj.SiteProvince,
                    prj.SitePostcode,
                    prj.ERPNumber,
                    prj.Township,
                    prj.PortionNumber,
                    prj.StandNumber,
                    prj.SalesPerson,
                    prj.SiteContactName,
                    prj.SiteContactPhone,
                    prj.GoogleMapUrl,
                    prj.SharePointUrl,
                    prj.SiteRentals,
                    prj.CrmStage,
                    prj.CrmNextAction,
                    prj.CrmFollowUpDate,
                    prj.Notes
                });

            // optional new files
            SaveProjectFiles(id, vm.Files);

            TempData["Message"] = "Project updated.";
            return RedirectToAction("Details", new { id });
        }

        // -------- tabs (stubs for now) --------
        [HttpGet]
        public PartialViewResult QuotesTab(int id)
        {
            ViewBag.ProjectId = id;
            return PartialView("Partials/_ProjectQuotes", model: null);
        }

        [HttpGet]
        public PartialViewResult AttachmentsTab(int id)
        {
            var docs = db.ProjectDocuments
                         .Where(d => d.ProjectId == id)
                         .OrderByDescending(d => d.UploadedAt)
                         .ToList();
            ViewBag.ProjectId = id;
            return PartialView("Partials/_ProjectAttachments", docs);
        }

        // File Upload Handlers
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadDocument(int id, IEnumerable<HttpPostedFileBase> files)
        {
            var prj = db.Projects.Find(id);
            if (prj == null) return HttpNotFound();

            var list = (files ?? Enumerable.Empty<HttpPostedFileBase>())
                       .Where(f => f != null && f.ContentLength > 0)
                       .ToList();

            if (!list.Any())
            {
                TempData["Error"] = "Please choose one or more files.";
                return RedirectToAction("Details", new { id, tab = "files" });
            }

            var result = SaveProjectFiles(id, list);   // <- helper below now targets /Uploads/ManualProjects/{ProjectCode}

            if (result.SavedCount > 0)
                TempData["Message"] = $"{result.SavedCount} file(s) uploaded.";
            if (result.Errors.Any())
                TempData["Error"] = string.Join("; ", result.Errors);

            return RedirectToAction("Details", new { id, tab = "files" });
        }

        private static string SanitizeForPath(string input, string fallback)
        {
            var s = (input ?? "").Trim();
            if (string.IsNullOrEmpty(s)) s = fallback;
            // allow letters/digits/underscore/hyphen/dot, replace others with _
            var chars = s.Select(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.' ? ch : '_').ToArray();
            s = new string(chars);
            if (string.IsNullOrWhiteSpace(s) || s.Trim('.').Length == 0) s = fallback;
            return s;
        }

        private sealed class SaveFilesResult
        {
            public int SavedCount { get; set; }
            public List<string> SavedNames { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
        }

        private SaveFilesResult SaveProjectFiles(int projectId, IEnumerable<HttpPostedFileBase> files)
        {
            var res = new SaveFilesResult();

            // Get project & code (for folder name)
            var prj = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == projectId);
            if (prj == null)
            {
                res.Errors.Add("Project not found.");
                return res;
            }
            var safeCode = SanitizeForPath(prj.ProjectCode, $"Project_{projectId}");

            // Target: ~/Uploads/ManualProjects/{ProjectCode}/
            var relDir = $"~/Uploads/ManualProjects/{safeCode}/";
            var absDir = Server.MapPath(relDir);
            Directory.CreateDirectory(absDir);

            var userName = GetUserDisplayName(GetCurrentUserId()) ?? "System";

            // constraints (tune as needed)
            const int MAX_BYTES = 20 * 1024 * 1024; // 20 MB
            var allowedExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { ".jpg",".jpeg",".png",".gif",".webp",".svg",".pdf",".doc",".docx",".xls",".xlsx",".txt" };

            foreach (var f in files)
            {
                try
                {
                    if (f == null || f.ContentLength <= 0) continue;
                    if (f.ContentLength > MAX_BYTES)
                    {
                        res.Errors.Add($"{Path.GetFileName(f.FileName)} is larger than {MAX_BYTES / (1024 * 1024)} MB.");
                        continue;
                    }

                    var original = Path.GetFileName(f.FileName) ?? "file";
                    var ext = Path.GetExtension(original) ?? "";

                    if (!string.IsNullOrEmpty(ext) && !allowedExts.Contains(ext))
                    {
                        res.Errors.Add($"{original}: file type not allowed.");
                        continue;
                    }

                    // sanitize base name
                    var baseName = Path.GetFileNameWithoutExtension(original) ?? "file";
                    var safeBase = SanitizeForPath(baseName, "file");

                    // unique filename
                    var unique = $"{safeBase}_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}_{Guid.NewGuid():N}{ext}";
                    var absPath = Path.Combine(absDir, unique);
                    f.SaveAs(absPath);

                    // app-relative, browser-accessible path (e.g., ~/Uploads/ManualProjects/P-00012/file.png)
                    var relPath = (VirtualPathUtility.Combine(relDir, unique) ?? "").Replace("\\", "/");

                    db.ProjectDocuments.Add(new ProjectDocument
                    {
                        ProjectId = projectId,
                        OriginalFileName = original,
                        StoredPath = relPath,
                        UploadedBy = userName,
                        UploadedAt = DateTime.UtcNow
                    });

                    res.SavedCount++;
                    res.SavedNames.Add(original);
                }
                catch (Exception ex)
                {
                    res.Errors.Add($"{Path.GetFileName(f?.FileName) ?? "file"}: {ex.Message}");
                }
            }

            if (res.SavedCount > 0)
            {
                db.SaveChanges();

                // audit summary
                var me = User as CustomPrincipal;
                var list = string.Join(", ", res.SavedNames.Take(5));
                if (res.SavedNames.Count > 5) list += $" …(+{res.SavedNames.Count - 5} more)";
                AuditLogger.Log(db, projectId, me.Id, AuditArea.Attachments,
                    $"Uploaded {res.SavedCount} file(s): {list}");
            }

            return res;
        }

        [HttpGet]
        public ActionResult DownloadDocument(int id, int? docId)
        {
            ProjectDocument doc;

            if (docId.HasValue && docId.Value > 0)
            {
                // id is projectId, docId is the document
                doc = db.ProjectDocuments.FirstOrDefault(d => d.Id == docId.Value && d.ProjectId == id);
            }
            else
            {
                // fallback: id is the docId
                doc = db.ProjectDocuments.FirstOrDefault(d => d.Id == id);
            }

            if (doc == null) return HttpNotFound();

            var absPath = Server.MapPath(doc.StoredPath ?? "");
            if (!System.IO.File.Exists(absPath)) return HttpNotFound();

            var downloadName = string.IsNullOrWhiteSpace(doc.OriginalFileName)
                ? System.IO.Path.GetFileName(absPath)
                : doc.OriginalFileName;

            var mime = System.Web.MimeMapping.GetMimeMapping(downloadName);
            return File(absPath, mime, downloadName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteDocument(int id, int? docId, string activeTab)
        {
            ProjectDocument doc;

            if (docId.HasValue && docId.Value > 0)
            {
                doc = db.ProjectDocuments.FirstOrDefault(d => d.Id == docId.Value && d.ProjectId == id);
            }
            else
            {
                // treat id as docId
                doc = db.ProjectDocuments.FirstOrDefault(d => d.Id == id);
            }

            if (doc == null)
            {
                TempData["Error"] = "File not found.";
                // if we have the project, redirect to it; else go back to Index
                return RedirectToAction("Details", new { id = (int?)doc?.ProjectId ?? 0, tab = "files" });
            }

            try
            {
                var abs = Server.MapPath(doc.StoredPath ?? "");
                if (System.IO.File.Exists(abs))
                    System.IO.File.Delete(abs);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Could not delete file from disk: {ex.Message}";
                return RedirectToAction("Details", new { id = doc.ProjectId, tab = "files" });
            }

            db.ProjectDocuments.Remove(doc);
            db.SaveChanges();

            // audit
            var me = User as CustomPrincipal;
            AuditLogger.Log(db, doc.ProjectId, me.Id, AuditArea.Attachments,
                $"Deleted file: {doc.OriginalFileName}");

            TempData["Message"] = "File deleted.";
            //return RedirectToAction("Details", new { id = doc.ProjectId, tab = "files" });
            return RedirectToAction("Details", new { id = doc.ProjectId, tab = activeTab ?? "general" });
        }

        // Sales Rep Dropdown On Project Create & Edit
        private IEnumerable<SelectListItem> BuildSalesRepOptions(int? selectedId = null)
        {
            return db.AppUsers
                .AsNoTracking()
                .Where(u => u.IsActive && u.IsSalesRep)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FullName,
                    Selected = (selectedId.HasValue && u.Id == selectedId.Value)
                })
                .ToList();
        }

        private int? FindSalesRepIdByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            return db.AppUsers
                     .AsNoTracking()
                     .Where(u => u.IsActive && u.IsSalesRep && u.FullName == name)
                     .Select(u => (int?)u.Id)
                     .FirstOrDefault();
        }

        private string FindSalesRepNameById(int? id)
        {
            if (!id.HasValue) return null;
            return db.AppUsers
                     .AsNoTracking()
                     .Where(u => u.Id == id.Value)
                     .Select(u => u.FullName)
                     .FirstOrDefault();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }

    // lightweight VM for the audit grid (user name shown, id saved)
    public class ProjectAuditListItem
    {
        public DateTime ChangedAt { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public int ChangedByUserId { get; set; }
        public string ChangedByName { get; set; }
    }
}
