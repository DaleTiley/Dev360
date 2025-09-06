using MillenniumWebFixed.Models;
using MillenniumWebFixed.Models.Projects;
using MillenniumWebFixed.Security;
using MillenniumWebFixed.Services;
using MillenniumWebFixed.ViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ProjectQuotesController : AppController
    {
        private readonly AppDbContext db = new AppDbContext();

        private IEnumerable<SelectListItem> GetDesignerOptions()
        {
            return db.AppUsers
                .Where(u => u.IsDesigner)   // only designers
                .OrderBy(u => u.Username)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Username
                })
                .ToList();
        }

        // -------------------------------
        // EDIT (GET) — uses QuoteEdit view
        // -------------------------------
        [Authorize]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var q = db.ProjectQuotes.AsNoTracking().FirstOrDefault(x => x.QuoteId == id);
            if (q == null) return HttpNotFound();

            var vm = new ProjectQuoteCreateVm
            {
                QuoteId = q.QuoteId,
                ProjectId = (int)q.ProjectId,
                QuoteNumber = q.QuoteNumber,
                BuildingType = q.BuildingType,
                QuotationOption = q.QuotationOption,
                Revision = q.Revision,
                SubmissionDate = q.SubmissionDate,
                PmrjbFileRef = q.PmrjbFileRef,
                LastImportDate = q.LastImportDate,
                QuantityOfUnits = q.QuantityOfUnits,
                DesignerId = q.DesignerId,
                RoofPitch = q.RoofPitch,
                RoofOverhang = q.RoofOverhang,
                GableOverhang = q.GableOverhang,
                RoofCovering = q.RoofCovering,
                TcLoading = q.TcLoading,
                BcLoading = q.BcLoading,
                WindSpeed = q.WindSpeed,
                TerrainCategory = q.TerrainCategory,
                MaxBattensCc = q.MaxBattensCc,
                MaxTrussesCc = q.MaxTrussesCc,
                RoofArea = q.RoofArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                FloorArea = q.FloorArea?.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Notes = q.Notes,
                Status = q.Status,
                DesignerOptions = GetDesignerOptions()
            };

            ModelState.Clear();
            return View("~/Views/Projects/Quote/QuoteEdit.cshtml", vm);
        }


        // -------------------------------
        // EDIT (POST) — never generates number
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProjectQuoteCreateVm vm)
        {
            var roof = TryParseDecimal(vm.RoofArea);
            var floor = TryParseDecimal(vm.FloorArea);

            // Optional: add errors if user typed something invalid
            if (roof == null && !string.IsNullOrWhiteSpace(vm.RoofArea))
                ModelState.AddModelError(nameof(vm.RoofArea), "Invalid number (e.g. 123.45 or 123,45)");
            if (floor == null && !string.IsNullOrWhiteSpace(vm.FloorArea))
                ModelState.AddModelError(nameof(vm.FloorArea), "Invalid number (e.g. 123.45 or 123,45)");

            if (!ModelState.IsValid)
            {
                // Clear stale attempted values so the view shows the normalized/current values
                ModelState.Remove("RoofArea");
                ModelState.Remove("FloorArea");

                vm.DesignerOptions = GetDesignerOptions();
                TempData["ToastMessage"] = "Quote save failed";
                TempData["ToastLevel"] = "danger";
                return View("~/Views/Projects/Quote/QuoteEdit.cshtml", vm);
            }


            var entity = db.ProjectQuotes.Find(vm.QuoteId);
            if (entity == null) return HttpNotFound();

            entity.ProjectId = vm.ProjectId;
            entity.BuildingType = vm.BuildingType;
            entity.QuotationOption = vm.QuotationOption;
            entity.Revision = vm.Revision;
            entity.SubmissionDate = vm.SubmissionDate;
            entity.PmrjbFileRef = vm.PmrjbFileRef;
            entity.LastImportDate = vm.LastImportDate;
            entity.QuantityOfUnits = vm.QuantityOfUnits;
            entity.DesignerId = vm.DesignerId;
            entity.RoofPitch = vm.RoofPitch;
            entity.RoofOverhang = vm.RoofOverhang;
            entity.GableOverhang = vm.GableOverhang;
            entity.RoofCovering = vm.RoofCovering;
            entity.TcLoading = vm.TcLoading;
            entity.BcLoading = vm.BcLoading;
            entity.WindSpeed = vm.WindSpeed;
            entity.TerrainCategory = vm.TerrainCategory;
            entity.MaxBattensCc = vm.MaxBattensCc;
            entity.MaxTrussesCc = vm.MaxTrussesCc;
            entity.RoofArea = roof;   // decimal?
            entity.FloorArea = floor;  // decimal?
            entity.Notes = vm.Notes;

            // No need to set state = Modified after Find(); EF tracks changes
            db.SaveChanges();

            // Toast payload
            TempData["ToastMessage"] = "Quote saved";
            TempData["ToastLevel"] = "success";

            // Go back to Project Details, General tab (adjust tab name if yours differs)
            return RedirectToAction("Edit", "ProjectQuotes", new { id = entity.QuoteId });  // ✅ stays on page
        }

        // --------------------------------
        // CLONE
        // --------------------------------
        // POST: /ProjectQuotes/CreateRevision/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRevision(int id)
        {
            var src = db.ProjectQuotes.SingleOrDefault(q => q.QuoteId == id);
            if (src == null) return HttpNotFound();

            // New revision & number: keep original prefix, just swap the suffix after the hyphen
            var newRev = NextRevision(src.Revision);
            var prefix = Regex.Replace(src.QuoteNumber ?? string.Empty, @"-.+$", string.Empty);
            var newNo = $"{prefix}-{newRev}";
            var userName = CurrentUserDisplay();
            var audit = BuildRevisionAudit(src, newNo, newRev, userName, DateTime.Now);

            var copy = new ProjectQuote
            {
                // Identity
                ProjectId = src.ProjectId,
                QuoteNumber = newNo,
                BuildingType = src.BuildingType,
                QuotationOption = src.QuotationOption,
                Revision = newRev,

                // Relations / ownership
                EnquiryId = src.EnquiryId,
                DesignerUserId = src.DesignerUserId,

                // Lifecycle
                Status = "Draft",                // or src.Status if you want to carry it over
                CreatedByUserId = User.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubmissionDate = src.SubmissionDate,     // set to null if new revision should reset submission
                LastImportDate = src.LastImportDate,

                // Amounts
                TotalAmount = src.TotalAmount,

                // Meta / design
                QuantityOfUnits = src.QuantityOfUnits,
                DesignerId = src.DesignerId,
                Designer = src.Designer,

                RoofPitch = src.RoofPitch,
                RoofOverhang = src.RoofOverhang,
                GableOverhang = src.GableOverhang,
                RoofCovering = src.RoofCovering,
                TcLoading = src.TcLoading,
                BcLoading = src.BcLoading,
                WindSpeed = src.WindSpeed,
                TerrainCategory = src.TerrainCategory,
                MaxBattensCc = src.MaxBattensCc,
                MaxTrussesCc = src.MaxTrussesCc,

                RoofArea = src.RoofArea,
                FloorArea = src.FloorArea,

                PmrjbFileRef = src.PmrjbFileRef,
                Notes = PrependAudit(src.Notes, audit)
            };

            db.ProjectQuotes.Add(copy);
            db.SaveChanges();

            // ---- Audit: quote revision created ----
            var projectId = copy.ProjectId ?? src.ProjectId ?? 0;

            AuditLogger.Log(
                db,
                projectId: projectId,
                userId: User.Id,
                area: AuditArea.Quote, // or AuditArea.General
                description: $"Quote revision created: {src.QuoteNumber} → {copy.QuoteNumber}",
                meta: new
                {
                    SourceQuoteId = src.QuoteId,
                    SourceQuoteNo = src.QuoteNumber,
                    NewQuoteId = copy.QuoteId,
                    NewQuoteNo = copy.QuoteNumber,
                    FromRevision = src.Revision,
                    ToRevision = copy.Revision
                }
            );

            var redirectUrl = Url.Action("Edit", new { id = copy.QuoteId, tab = "quote" });
            var isAjax = Request.IsAjaxRequest() ||
                         string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            return isAjax
                ? (ActionResult)Json(new { ok = true, redirect = redirectUrl })
                : Redirect(redirectUrl);
        }

        // NOTE: keep the path you already use for the modal
        private const string ClonePartialPath = "~/Views/Projects/Quote/_CloneQuoteModal.cshtml";

        // GET: /ProjectQuotes/Clone/{id}
        [HttpGet]
        public ActionResult Clone(int id)
        {
            var src = db.ProjectQuotes.AsNoTracking().FirstOrDefault(q => q.QuoteId == id);
            if (src == null)
            {
                if (Request.IsAjaxRequest())    // when the modal lazy-loads, give a tiny html error
                    return Content("<div class='p-4 text-danger small'>Source quote not found.</div>");

                return HttpNotFound();
            }

            var vm = new CloneQuoteVm
            {
                SourceQuoteId = src.QuoteId,
                BuildingType = src.BuildingType,
                QuotationOption = src.QuotationOption
            };
            return PartialView(ClonePartialPath, vm);
        }

        // POST: /ProjectQuotes/Clone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Clone(CloneQuoteVm vm)
        {
            if (!ModelState.IsValid)
                return PartialView(ClonePartialPath, vm);

            var src = db.ProjectQuotes.FirstOrDefault(q => q.QuoteId == vm.SourceQuoteId);
            if (src == null)
            {
                ModelState.AddModelError("", "Source quote not found.");
                return PartialView(ClonePartialPath, vm);
            }

            // Normalize
            string bt = NormalizeBuildingType(string.IsNullOrWhiteSpace(vm.BuildingType) ? src.BuildingType : vm.BuildingType);
            int opt = NormalizeOption(vm.QuotationOption ?? src.QuotationOption);
            string rev = "A";

            // Generate new quote number
            int projectId = src.ProjectId.GetValueOrDefault();
            string newQuoteNo = GenerateQuoteNo(projectId, bt, opt, rev);

            // Build clone (copy your fields)
            var copy = new ProjectQuote
            {
                ProjectId = src.ProjectId,
                QuoteNumber = newQuoteNo,
                BuildingType = bt,
                QuotationOption = opt,
                Revision = rev,

                EnquiryId = src.EnquiryId,
                DesignerUserId = src.DesignerUserId,

                Status = "Draft",
                CreatedByUserId = (User as CustomPrincipal)?.Id ?? src.CreatedByUserId, // <- avoids compile errors if User.Id not available
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                SubmissionDate = src.SubmissionDate,
                LastImportDate = src.LastImportDate,

                TotalAmount = src.TotalAmount,

                QuantityOfUnits = src.QuantityOfUnits,
                DesignerId = src.DesignerId,
                Designer = src.Designer,

                RoofPitch = src.RoofPitch,
                RoofOverhang = src.RoofOverhang,
                GableOverhang = src.GableOverhang,
                RoofCovering = src.RoofCovering,
                TcLoading = src.TcLoading,
                BcLoading = src.BcLoading,
                WindSpeed = src.WindSpeed,
                TerrainCategory = src.TerrainCategory,
                MaxBattensCc = src.MaxBattensCc,
                MaxTrussesCc = src.MaxTrussesCc,
                RoofArea = src.RoofArea,
                FloorArea = src.FloorArea,

                PmrjbFileRef = src.PmrjbFileRef,
                Notes = PrependAudit(src.Notes, $"Cloned from {src.QuoteNumber} on {DateTime.Now:g}")
            };

            db.ProjectQuotes.Add(copy);
            db.SaveChanges();

            // ---- Audit: quote cloned ----
            AuditLogger.Log(
                db,
                projectId: projectId,                // same project as the source
                userId: User.Id,                     // who performed the clone
                area: AuditArea.Quote,               // or AuditArea.General if you prefer
                description: $"Quote cloned: {src.QuoteNumber} → {copy.QuoteNumber}",
                meta: new
                {
                    SourceQuoteId = src.QuoteId,
                    SourceQuoteNo = src.QuoteNumber,
                    NewQuoteId = copy.QuoteId,
                    NewQuoteNo = copy.QuoteNumber,
                    FromRevision = src.Revision,     // usually something like "A", "B", ...
                    ToRevision = copy.Revision,    // resets to "A" on clone
                    BuildingTypeOld = src.BuildingType,
                    BuildingTypeNew = copy.BuildingType,
                    OptionOld = src.QuotationOption,
                    OptionNew = copy.QuotationOption
                }
            );

            var redirectUrl = Url.Action("Edit", new { id = copy.QuoteId, tab = "quote" });
            bool isAjax = Request.IsAjaxRequest()
                       || string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

            return isAjax ? (ActionResult)Json(new { ok = true, redirect = redirectUrl })
                          : Redirect(redirectUrl);
        }


        // GET: /ProjectQuotes/Create?projectId=5
        [HttpGet]
        public ActionResult Create(int projectId)
        {
            var vm = new ProjectQuoteCreateVm
            {
                ProjectId = projectId,
                Revision = "A",                 // default first revision
                QuotationOption = 1,            // sensible default
                Status = "Draft"
            };

            ModelState.Clear();
            return View("~/Views/Projects/Quote/QuoteCreate.cshtml", vm);
        }

        // POST: /ProjectQuotes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProjectQuoteCreateVm vm)
        {
            if (!ModelState.IsValid)
                return View("QuoteCreate", vm);

            // Normalize inputs
            var buildingType = NormalizeBuildingType(vm.BuildingType);
            var option = NormalizeOption(vm.QuotationOption);
            var revision = NormalizeRevision(vm.Revision);

            // Generate number for NEW (sequence + bt + option + -rev)
            var quoteNo = GenerateQuoteNo(vm.ProjectId, buildingType, option, revision);

            // Build entity — map ALL your fields
            var me = (User as CustomPrincipal);
            var row = new ProjectQuote
            {
                ProjectId = vm.ProjectId,
                QuoteNumber = quoteNo,

                // identity fields
                BuildingType = buildingType,
                QuotationOption = option,
                Revision = revision,

                // ownership/status
                EnquiryId = 1, //vm.EnquiryId,
                DesignerUserId = vm.DesignerId,
                Status = string.IsNullOrWhiteSpace(vm.Status) ? "Draft" : vm.Status.Trim(),

                // amounts
                TotalAmount = vm.TotalAmount, // keep if decimal? in VM; otherwise parse as needed

                // dates
                SubmissionDate = vm.SubmissionDate,
                LastImportDate = vm.LastImportDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,

                // created by
                CreatedByUserId = User.Id,  // or whatever your CustomPrincipal exposes

                // design/meta
                QuantityOfUnits = vm.QuantityOfUnits,
                DesignerId = vm.DesignerId,
                Designer = vm.Designer,

                RoofPitch = vm.RoofPitch,
                RoofOverhang = vm.RoofOverhang,
                GableOverhang = vm.GableOverhang,
                RoofCovering = vm.RoofCovering,
                TcLoading = vm.TcLoading,
                BcLoading = vm.BcLoading,
                WindSpeed = vm.WindSpeed,
                TerrainCategory = vm.TerrainCategory,
                MaxBattensCc = vm.MaxBattensCc,
                MaxTrussesCc = vm.MaxTrussesCc,

                // areas (parse from string if your VM uses string fields)
                RoofArea = TryParseDecimal(vm.RoofArea),
                FloorArea = TryParseDecimal(vm.FloorArea),

                PmrjbFileRef = vm.PmrjbFileRef,
                Notes = vm.Notes
            };

            db.ProjectQuotes.Add(row);
            db.SaveChanges();

            // After db.ProjectQuotes.Add(pq); and db.SaveChanges();

            var userId = (User as CustomPrincipal)?.Id ?? 0;  // safe guard if User.Id not exposed

            AuditLogger.Log(
                db,
                projectId: vm.ProjectId,
                userId: userId,
                area: AuditArea.Quote,                         // or your fallback area
                description: $"Quote created: {vm.QuoteNumber}",
                meta: new
                {
                    QuoteId = vm.QuoteId,
                    QuoteNo = vm.QuoteNumber,
                    BuildingType = vm.BuildingType,
                    QuotationOption = vm.QuotationOption,
                    Revision = vm.Revision,              // likely "A" on create
                    Status = vm.Status,                // e.g. "Draft"
                    TotalAmount = vm.TotalAmount,
                    CreatedAtUtc = vm.CreatedAt,
                    CreatedByUserId = User.Id
                }
            );

            TempData["Message"] = "Quote created.";
            return RedirectToAction("Edit", new { id = row.QuoteId, tab = "quote" });
        }

        // -----------------------------------
        // Project Quote Line Items
        // -----------------------------------
        // ImportController (or ProjectQuotesController)
        // GET: /Import/GetQuoteLines?id={quoteId}&projectId={optional}
        [HttpGet]
        [Route("ProjectQuotes/GetQuoteLines")]
        public JsonResult GetQuoteLines(int id, int? projectId = null) // id = QuoteId
        {
            var q = db.ProjectQuoteLineItems.Where(x => x.QuoteId == id);
            if (projectId.HasValue) q = q.Where(x => x.ProjectId == projectId.Value);

            var rows = q.Select(x => new QuoteLineRowVm
            {
                ProductIdExcel = x.ProductIdExcel,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                PricePerUnit = x.PricePerUnit,
                CostPerUnit = x.CostPerUnit,
                Vat = x.Vat,
                PriceOverridden = x.PriceOverridden,
                LineNotes = x.LineNotes            // <-- key name matches the JS
            }).ToList();

            // summary
            decimal subtotalEx = 0m, vatAmt = 0m;
            foreach (var r in rows)
            {
                var qty = r.Quantity ?? 0m;
                var unit = r.PricePerUnit ?? 0m;
                var rate = NormalizeVat(r.Vat);
                var ex = qty * unit;
                subtotalEx += ex;
                vatAmt += ex * rate;
            }
            var totalInc = subtotalEx + vatAmt;

            return Json(new
            {
                ok = true,
                Rows = rows,
                Summary = new
                {
                    LinesCount = rows.Count,
                    Subtotal = subtotalEx,
                    Vat = vatAmt,
                    Total = totalInc
                }
            }, JsonRequestBehavior.AllowGet);
        }


        // ---------------------------------
        // Quote Approval
        // ---------------------------------
        // GET: approvers for the modal
        [HttpGet]
        public ActionResult GetApprovers()
        {
            try
            {
                var users = db.AppUsers
                    .AsNoTracking()
                    .Where(u => u.IsActive && u.IsQuoteApprover)
                    .Select(u => new
                    {
                        u.Id,
                        Name = (u.FullName ?? u.Username).Trim(),
                        Email = (u.Email ?? "").Trim(),
                        Designation = (u.Designation ?? "").Trim(),
                        // If Department is a string column:
                        Department = (u.Department ?? "").Trim()
                        // If Department is a related entity, use this instead:
                        // Department = (u.DepartmentNavigation != null ? u.DepartmentNavigation.Name : "").Trim()
                    })
                    .OrderBy(u => u.Name)
                    .ToList();

                return Json(users, JsonRequestBehavior.AllowGet);
            }
            catch (Exception x)
            {
                Debug.WriteLine(x);
                return new HttpStatusCodeResult(500);
            }
        }


        // POST: send for approval (set quote to PendingApproval, email approvers, log)
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SendForApproval(int quoteId, int[] approverIds)
        {
            try
            {
                if (approverIds == null || approverIds.Length == 0)
                    return Json(new { success = false, message = "Please select at least one approver." });

                var quote = db.ProjectQuotes.SingleOrDefault(q => q.QuoteId == quoteId);
                if (quote == null)
                    return Json(new { success = false, message = "Quote not found." });

                var meName = User?.Identity?.Name ?? "System";
                var meEmail = ""; // set from your user context if available
                var meRealName = User.FullName;
                var approvers = db.AppUsers
                    .Where(u => u.IsActive && approverIds.Contains(u.Id ?? 0))
                    .ToList();

                if (approvers.Count == 0)
                    return Json(new { success = false, message = "No valid approvers found." });

                var toList = string.Join(";", approvers
                    .Where(a => !string.IsNullOrWhiteSpace(a.Email))
                    .Select(a => a.Email.Trim()));

                // Update quote submit fields + status
                quote.Status = "PendingApproval";
                quote.SubmittedForApprovalBy = meName;
                quote.SubmittedForApprovalOn = DateTime.UtcNow;
                quote.SubmittedForApprovalTo = toList;

                // Log SUBMIT
                db.QuoteWorkflowLogs.Add(new QuoteWorkflowLog
                {
                    QuoteId = quoteId,
                    Event = "Submit",
                    ActorUserId = db.AppUsers.Where(u => u.Username == meName).Select(u => u.Id).FirstOrDefault(),
                    ActorName = meName,
                    ActorEmail = meEmail,
                    Notes = $"Submitted to: {toList}",
                    MetaJson = Newtonsoft.Json.JsonConvert.SerializeObject(new { ApproverIds = approverIds })
                });

                db.SaveChanges();
                var editUrl = Url.Action("Edit", "ProjectQuotes", new { id = quote.QuoteId }, Request?.Url?.Scheme);
                var loginUrl = Url.Action("Login", "Account", new { returnUrl = editUrl }, Request?.Url?.Scheme);

                foreach (var a in approvers)
                {
                    if (string.IsNullOrWhiteSpace(a.Email)) continue;

                    var ok = EmailService.SendQuoteApprovalRequest(
                        toEmail: a.Email,
                        approverName: a.FullName ?? a.Username,
                        quoteId: quote.QuoteId,
                        projectName: quote.Project.ProjectCode,
                        approveUrl: loginUrl,           // button
                        requestedByName: meName,
                        requestedByEmail: meEmail,
                        requesterFullname: User.FullName,
                        out var err,
                        bcc: ConfigurationManager.AppSettings["QuoteEmailBcc"],
                        cc: null,
                        plainUrl: editUrl               // copy/paste link
                    );

                    db.QuoteWorkflowLogs.Add(new QuoteWorkflowLog
                    {
                        QuoteId = quote.QuoteId,
                        Event = ok ? "Notify" : "NotifyFailed",
                        ActorName = meName,
                        ActorEmail = meEmail,
                        Notes = ok
                            ? $"Email sent to {a.FullName ?? a.Username} <{a.Email}>"
                            : $"Email FAILED to {a.FullName ?? a.Username} <{a.Email}>: {err}"
                    });
                }
                db.SaveChanges();

                TempData["Message"] = $"Approval Email sent.";
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to submit for approval." });
            }
        }

        // POST: approver approves (sets ApprovedBy/On, Status=Approved, log)
        [HttpPost, ValidateAntiForgeryToken /*, Authorize(Roles = "QuoteApprover,Admin")*/]
        public ActionResult ApproveQuote(int quoteId)
        {
            try
            {
                var quote = db.ProjectQuotes.SingleOrDefault(q => q.QuoteId == quoteId);
                if (quote == null)
                    return Json(new { success = false, message = "Quote not found." });

                var approverName = User?.Identity?.Name ?? "Approver";
                var approverEmail = ""; // set from your user context if available

                quote.Status = "Approved";
                quote.ApprovedBy = approverName;
                quote.ApprovedOn = DateTime.UtcNow;

                db.QuoteWorkflowLogs.Add(new QuoteWorkflowLog
                {
                    QuoteId = quoteId,
                    Event = "Approve",
                    ActorName = approverName,
                    ActorEmail = approverEmail,
                    Notes = "Quote approved from UI"
                });

                db.SaveChanges();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "Failed to approve quote." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TakeApprovalDecision(int quoteId, bool approve, string notes)
        {
            try
            {
                var meName = User?.Identity?.Name ?? "User";
                var meEmail = ""; // set from your identity/user if you store it

                var approver = db.AppUsers.FirstOrDefault(u => u.Username == meName);
                if (approver == null || !approver.IsQuoteApprover)
                    return Json(new { success = false, message = "Not authorized to approve/reject." });

                var quote = db.ProjectQuotes.FirstOrDefault(q => q.QuoteId == quoteId);
                if (quote == null) return Json(new { success = false, message = "Quote not found." });

                if (!string.Equals(quote.Status, "PendingApproval", StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "Quote is not in Pending Approval state." });

                var newStatus = approve ? "Approved" : "Rejected";
                quote.Status = newStatus;

                if (approve)
                {
                    // If you added these columns earlier:
                    quote.ApprovedBy = meName;
                    quote.ApprovedOn = DateTime.UtcNow;
                }
                else
                {
                    // If you prefer to bounce back to Draft instead of 'Rejected', swap:
                    // newStatus = "Draft"; quote.Status = "Draft";
                }

                db.QuoteWorkflowLogs.Add(new QuoteWorkflowLog
                {
                    QuoteId = quoteId,
                    Event = approve ? "Approve" : "Reject",
                    ActorUserId = approver.Id,
                    ActorName = meName,
                    ActorEmail = meEmail,
                    Notes = (approve ? "Approved" : "Rejected") + (string.IsNullOrWhiteSpace(notes) ? "" : $": {notes}"),
                    MetaJson = Newtonsoft.Json.JsonConvert.SerializeObject(new { Approve = approve, Notes = notes })
                });

                db.SaveChanges();

                // Optional: notify the requester here with EmailService if you track who created it.

                // Return a nice display label (you already have a status prettifier on the view; mirror here)
                var display = approve ? "Approved" : "Rejected";
                return Json(new { success = true, newStatus = display });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        private static decimal NormalizeVat(decimal? v)
        {
            if (!v.HasValue) return 0m;
            var n = v.Value;
            return n >= 1m ? n / 100m : n; // accept 15 or 0.15
        }

        // VM used above
        public class QuoteLineRowVm
        {
            public string ProductIdExcel { get; set; }
            public string ProductName { get; set; }
            public decimal? Quantity { get; set; }
            public decimal? PricePerUnit { get; set; }
            public decimal? CostPerUnit { get; set; }
            public decimal? Vat { get; set; }
            public bool? PriceOverridden { get; set; }
            public string LineNotes { get; set; }
        }

        public class QuoteLineSummaryVm
        {
            public int LinesCount { get; set; }
            public decimal Subtotal { get; set; } // ex VAT
            public decimal Vat { get; set; }
            public decimal Total { get; set; } // inc VAT
        }

        #region Helpers
        // ======= Quote-Line PARSERS =======
        private List<QuoteLineImportDto> ParseCsvQuoteLines(Stream stream)
        {
            var rows = new List<QuoteLineImportDto>();
            stream.Position = 0;
            using (var sr = new StreamReader(stream))
            {
                string header = sr.ReadLine(); // skip header
                int lineNo = 1;
                while (!sr.EndOfStream)
                {
                    lineNo++;
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // naive split; if you need quoted fields use CsvHelper later
                    var parts = line.Split(',');
                    if (parts.Length < 7)
                        throw new InvalidOperationException($"Line {lineNo}: expected at least 7 columns.");

                    decimal ParseDec(string s, string name)
                    {
                        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
                        if (decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out d)) return d;
                        throw new InvalidOperationException($"Line {lineNo}: {name} not a number ('{s}').");
                    }

                    var dto = new QuoteLineImportDto
                    {
                        Section = parts[0]?.Trim(),
                        Group = parts[1]?.Trim(),
                        ItemCode = parts[2]?.Trim(),
                        Description = parts[3]?.Trim(),
                        Uom = parts[4]?.Trim(),
                        Qty = ParseDec(parts[5], "Qty"),
                        UnitSell = ParseDec(parts[6], "UnitSell"),
                        SortOrder = parts.Length >= 8 && int.TryParse(parts[7], out var so) ? (int?)so : null
                    };
                    rows.Add(dto);
                }
            }
            return rows;
        }

        private List<QuoteLineImportDto> ParseXlsxQuoteLines(ExcelWorksheet sheet)
        {
            // Expect header row on row 1, data from row 2
            var rows = new List<QuoteLineImportDto>();
            var dim = sheet.Dimension;
            if (dim == null || dim.End.Row < 2) return rows;

            int FindCol(string name)
            {
                for (int c = 1; c <= dim.End.Column; c++)
                {
                    if (string.Equals(sheet.Cells[1, c].Text.Trim(), name, System.StringComparison.OrdinalIgnoreCase))
                        return c;
                }
                return -1;
            }

            int cSection = FindCol("Section");
            int cGroup = FindCol("Group");
            int cCode = FindCol("ItemCode");
            int cDesc = FindCol("Description");
            int cUom = FindCol("UOM");
            int cQty = FindCol("Qty");
            int cUnitSell = FindCol("UnitSell");
            int cSort = FindCol("SortOrder");

            string Need(int col, string label) => col > 0 ? null : label;
            var missing = new[] {
        Need(cSection,"Section"), Need(cGroup,"Group"), Need(cCode,"ItemCode"), Need(cDesc,"Description"),
        Need(cUom,"UOM"), Need(cQty,"Qty"), Need(cUnitSell,"UnitSell")
    }.Where(x => x != null).ToList();

            if (missing.Any())
                throw new InvalidOperationException("Missing required columns: " + string.Join(", ", missing));

            for (int r = 2; r <= dim.End.Row; r++)
            {
                if (sheet.Cells[r, cCode].Value == null && sheet.Cells[r, cDesc].Value == null) continue;

                decimal ParseDec(int col, string name)
                {
                    var t = sheet.Cells[r, col].Text?.Trim();
                    if (string.IsNullOrEmpty(t)) return 0m;
                    if (decimal.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return d;
                    if (decimal.TryParse(t, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out d)) return d;
                    throw new InvalidOperationException($"Row {r}: {name} not a number ('{t}').");
                }

                int? ParseInt(int col)
                {
                    var t = sheet.Cells[r, col].Text?.Trim();
                    return int.TryParse(t, out var i) ? (int?)i : null;
                }

                rows.Add(new QuoteLineImportDto
                {
                    Section = sheet.Cells[r, cSection].Text?.Trim(),
                    Group = sheet.Cells[r, cGroup].Text?.Trim(),
                    ItemCode = sheet.Cells[r, cCode].Text?.Trim(),
                    Description = sheet.Cells[r, cDesc].Text?.Trim(),
                    Uom = sheet.Cells[r, cUom].Text?.Trim(),
                    Qty = ParseDec(cQty, "Qty"),
                    UnitSell = ParseDec(cUnitSell, "UnitSell"),
                    SortOrder = cSort > 0 ? ParseInt(cSort) : null
                });
            }
            return rows;
        }

        // Build a display name for audit
        private string CurrentUserDisplay()
        {

            return !string.IsNullOrWhiteSpace(User.FullName)
                ? User.FullName
                : (!string.IsNullOrWhiteSpace(User?.Identity?.Name) ? User.Identity.Name : $"UserId:{User.Id}");
        }

        // Prepend an audit line to Notes (caps at 2000 chars)
        private string PrependAudit(string existingNotes, string line)
        {
            var trimmed = (existingNotes ?? "").TrimStart();
            var combined = string.IsNullOrWhiteSpace(trimmed)
                ? line
                : line + Environment.NewLine + trimmed;

            // hard cap at 2000 to match [MaxLength(2000)]
            return combined.Length <= 2000 ? combined : combined.Substring(0, 2000);
        }

        private string BuildCloneAudit(ProjectQuote src, string newNo, string newBt, int newOpt, string user, DateTime when)
        {
            var changes = new List<string>();
            if (!string.Equals(src.BuildingType, newBt, StringComparison.OrdinalIgnoreCase))
                changes.Add($"BuildingType {src.BuildingType}→{newBt}");
            if ((src.QuotationOption ?? 1) != newOpt)
                changes.Add($"Option {src.QuotationOption}→{newOpt}");

            var changeStr = changes.Count > 0 ? "  " + string.Join("; ", changes) : "";
            return $"{when:yyyy-MM-dd HH:mm}  CLONE: From {src.QuoteNumber} → {newNo}. By {user}.{changeStr}";
        }

        private string BuildRevisionAudit(ProjectQuote src, string newNo, string newRev, string user, DateTime when)
        {
            var fromRev = string.IsNullOrWhiteSpace(src.Revision) ? "?" : src.Revision.ToUpperInvariant();
            return $"{when:yyyy-MM-dd HH:mm}  REVISION: {fromRev}→{newRev}, {src.QuoteNumber} → {newNo}. By {user}.";
        }
        private string GenerateQuoteNo(int projectId, string buildingType, int option, string revision)
        {
            var yy = DateTime.UtcNow.ToString("yy");
            var seq = GetNextProjectSequence(projectId, pad: 4);   // 0001, 0002,...
            var bt = NormalizeBuildingType(buildingType);
            var opt = option.ToString(CultureInfo.InvariantCulture);
            var rev = NormalizeRevision(revision);
            return $"Q{yy}{seq}{bt}{opt}-{rev}";
        }

        private string GetNextProjectSequence(int projectId, int pad = 4)
        {
            var list = db.ProjectQuotes
                         .Where(q => q.ProjectId == projectId && q.QuoteNumber.StartsWith("Q"))
                         .Select(q => q.QuoteNumber)
                         .ToList();

            var max = 0;
            var rx = new Regex(@"^Q\d{2}(\d+)"); // capture the numeric sequence after Qyy
            foreach (var qn in list)
            {
                var m = rx.Match(qn ?? "");
                if (m.Success && int.TryParse(m.Groups[1].Value, out var n) && n > max)
                    max = n;
            }
            return (max + 1).ToString(new string('0', pad)); // D4
        }

        private static string NextRevision(string current)
        {
            if (string.IsNullOrWhiteSpace(current)) return "A";
            var s = current.Trim().ToUpperInvariant();

            // numeric? => bump
            if (int.TryParse(s, out var n))
                return (n + 1).ToString();

            // letters? => Excel-like increment (A..Z..AA..AB..)
            if (Regex.IsMatch(s, "^[A-Z]+$"))
            {
                var chars = s.ToCharArray();
                int i = chars.Length - 1;

                // carry over Z's to A's from right to left
                while (i >= 0 && chars[i] == 'Z')
                {
                    chars[i] = 'A';
                    i--;
                }

                if (i >= 0)
                {
                    chars[i]++; // bump first non-Z
                    var next = new string(chars);
                    return next.Length <= 2 ? next : "ZZ"; // cap to 2 if you want
                }

                // overflow (e.g., "Z" -> "AA")
                var result = "A" + new string(chars); // chars are all 'A' now
                return result.Length <= 2 ? result : "ZZ"; // cap to 2 letters
            }

            // fallback
            return "B";
        }

        private static decimal? TryParseDecimal(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim()
                 .Replace(" ", "")
                 .Replace("\u00A0", "")
                 .Replace("\u202F", "")
                 .Replace(',', '.');
            return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : (decimal?)null;
        }

        [HttpGet]
        public JsonResult NextSeq(int projectId)
        {
            var now = DateTime.UtcNow;
            var yy = now.Year % 100;

            var seq = db.ProjectQuotes
                .Where(q => q.ProjectId == projectId &&
                            q.CreatedAt.HasValue &&
                            q.CreatedAt.Value.Year == now.Year)
                .Count() + 1;

            return Json(new { yy, seq }, JsonRequestBehavior.AllowGet);
        }
        private static string TwoDigitYear(int year) => (year % 100).ToString("00");

        private static string BuildingTypeCode(string buildingType)
        {
            if (string.IsNullOrWhiteSpace(buildingType)) return "A";
            return buildingType.Trim().ToUpperInvariant().Substring(0, 1);
        }

        private static string NormalizeBuildingType(string bt) => (bt ?? "").Trim().ToUpperInvariant();

        private static int NormalizeOption(int? opt) => (opt == null || opt < 1) ? 1 : opt.Value;

        private static string NormalizeRevision(string rev)
        {
            if (string.IsNullOrWhiteSpace(rev)) return "A";
            rev = rev.Trim();

            // letters (1–2) => uppercase
            if (Regex.IsMatch(rev, "^[A-Za-z]{1,2}$"))
                return rev.ToUpperInvariant();

            // digits (1–3) => keep numeric form (strip leading zeros)
            if (Regex.IsMatch(rev, "^[0-9]{1,3}$"))
                return Regex.Replace(rev, "^0+(?=\\d)", ""); // "01" -> "1"

            // fallback
            return "A";
        }

        private string GenerateNextProjectQuoteNumber(int projectId, string buildingType, int? option, string revision)
        {
            var now = DateTime.UtcNow;
            var seq = db.ProjectQuotes
                .Where(q => q.ProjectId == projectId &&
                            q.CreatedAt.HasValue &&
                            q.CreatedAt.Value.Year == now.Year)
                .Count() + 1;

            var yy = TwoDigitYear(now.Year);
            var nnnn = seq.ToString("0000");
            var b = BuildingTypeCode(buildingType);
            var o = NormalizeOption(option);
            var r = NormalizeRevision(revision);

            return $"Q{yy}{nnnn}{b}{o}-{r}";
        }

        #endregion
    }
}
