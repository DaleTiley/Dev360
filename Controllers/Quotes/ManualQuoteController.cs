using MillenniumWebFixed.Models;
using MillenniumWebFixed.PdfTemplates;
using MillenniumWebFixed.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ManualQuoteController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // Dummy Test
        public ActionResult TestGeneratePdf()
        {
            //https://localhost:44394/ManualQuote/TestGeneratePdf
            int testQuoteId = 1069;
            string outputDir = Server.MapPath("~/PdfTemplates/GeneratedQuotes");
            string quoteNumber = "Q-TEST";
            string revisionID = "A";

            var generator = new QuotePdfGenerator();
            var pdfPath = generator.GenerateManualQuotePdf(testQuoteId, outputDir, quoteNumber, revisionID);

            return File(pdfPath, "application/pdf", Path.GetFileName(pdfPath));
        }

        // Grouped Quote List
        public ActionResult ListManualQuotesGrouped()
        {
            // Pull the rows once (no tracking = faster for read-only lists)
            var all = db.ManualQuotes
                .Include(q => q.PotentialCustomer)
                .Include(q => q.SalesRep)
                .AsNoTracking()
                .ToList();

            // Group by QuoteNo and shape for the grid
            var model = all
                .GroupBy(q => q.QuoteNo)
                .Select(g =>
                {
                    var latest = g.OrderByDescending(x => x.ImportDate).First();
                    return new ManualQuoteSummaryVM
                    {
                        QuoteNo = g.Key,
                        RevCount = g.Count(),
                        LastSaved = g.Max(x => x.CreatedAt),
                        LatestQuoteId = latest.Id,
                        LatestRevision = latest.RevisionID,
                        CustomerName = latest.PotentialCustomer != null ? latest.PotentialCustomer.FullName : "N/A",
                        SiteName = latest.SiteName,
                        SalesRepName = latest.SalesRep != null ? latest.SalesRep.FullName : null,
                        Revisions = g.OrderBy(x => x.ImportDate)
                                          .Select(x => new ManualQuoteRevisionVM
                                          {
                                              Id = x.Id,
                                              Rev = x.RevisionID,
                                              Saved = x.CreatedAt
                                          })
                                          .ToList()
                    };
                })
                .OrderByDescending(x => x.LastSaved)
                .ToList();

            return View("ListManualQuotes_Grouped", model);
        }

        public ActionResult ManualQuote_Tabs(int? id)
        {
            var result = ManualQuote(id) as ViewResult;
            if (result == null || result.Model == null) return HttpNotFound();

            var vm = result.Model as ManualQuoteViewModel;
            if (vm != null)
                vm.QuoteImageCount = db.QuoteImages.Count(q => q.QuoteId == vm.Id);

            result.ViewName = "ManualQuote_Tabs";
            return result;
        }

        // GET: ManualQuote
        public ActionResult ManualQuote(int? id)
        {
            var vm = new ManualQuoteViewModel();

            // Load shared dropdowns and stock list
            vm.Customers = db.Customers
                .Where(c => c.IsActive)
                .Select(c => new CustomerDropdownItem
                {
                    Id = c.Id,
                    CustomerName = c.CustomerName,
                    ContactPerson = c.ContactPerson,
                    Email = c.Email,
                    CustomerCode = c.CustomerCode,
                    AddressLine1 = c.AddressLine1,
                    AddressLine2 = c.AddressLine2,
                    City = c.City,
                    StateProvince = c.StateProvince,
                    Phone = c.Phone
                }).ToList();

            ViewBag.CustomerList = db.Customers
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CustomerName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CustomerName
                    }).ToList();

            vm.Contacts = db.Contacts
                .Select(c => new ContactDropdownItem
                {
                    ContactId = c.ContactId,
                    Name = c.Name,
                    Designation = c.Designation,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    CustomerId = c.CustomerId
                }).ToList();

            vm.Enquiries = db.Enquiries
                .Select(e => new EnquiryDropdownItem
                {
                    EnquiryId = e.EnquiryId,
                    EnquiryNumber = e.EnquiryNumber,
                    CustomerName = e.Customer.CustomerName
                }).ToList();

            vm.StockItems = db.Products
                .Where(p => p.IsActive)
                .Select(p => new StockItemViewModel
                {
                    ProductId = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    ItemTypeName = p.ItemType.Name,
                    BaseUOM = p.BaseUOM
                }).ToList();

            vm.DesignerOptions = db.AppUsers
                .Where(u => u.IsActive && u.IsDesigner)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FullName
                }).ToList();

            vm.SalesRepOptions = db.AppUsers
                .Where(u => u.IsActive && u.IsSalesRep)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.FullName
                }).ToList();

            // ===========================
            // Load existing quote if ID
            // ===========================
            if (id.HasValue)
            {
                // 1) Pull just the fields you need (no big Includes)
                var data = db.ManualQuotes
                    .AsNoTracking()
                    .Where(q => q.Id == id.Value)
                    .Select(q => new
                    {
                        q.Id,
                        q.QuoteNo,
                        q.RevisionID,
                        q.ImportDate,
                        q.QuantityOfUnits,
                        q.DueDate,
                        q.DesignerId,
                        q.ImportedPMRJBFileRef,

                        q.SiteName,
                        q.StandNumber,
                        q.ProjectName,
                        q.UnitBlockNumber,
                        q.UnitBlockType,
                        q.StreetAddress,

                        q.PotentialCustomerId,
                        q.SalesRepId,
                        q.ContactId,
                        q.EnquiryId,

                        q.ShipToStreet1,
                        q.ShipToStreet2,
                        q.ShipToStreet3,
                        q.ShipToCity,
                        q.ShipToState,
                        q.ShipToPostalCode,
                        q.ShipToCountry,

                        q.RoofPitch,
                        q.RoofOverhang,
                        q.RoofGableOverhang,
                        q.RoofCovering,
                        q.MaxBattenCenters,
                        q.MaxTrustCenters,
                        q.FloorArea,
                        q.RoofArea,

                        q.QuoteNotes,
                        q.SalesNotes,

                        // Display names via correlated subqueries (EF translates to LEFT JOINs)
                        PotentialCustomerName = db.Customers
                            .Where(c => c.Id == q.PotentialCustomerId)
                            .Select(c => c.CustomerName)
                            .FirstOrDefault(),

                        SelectedContactName = db.Contacts
                            .Where(c => c.ContactId == q.ContactId)
                            .Select(c => c.Name)
                            .FirstOrDefault(),

                        SelectedEnquiryNumber = db.Enquiries
                            .Where(e => e.EnquiryId == q.EnquiryId)
                            .Select(e => e.EnquiryNumber)
                            .FirstOrDefault()
                    })
                    .FirstOrDefault();

                if (data == null) return HttpNotFound();

                // 2) Map to your ViewModel (convert ints to strings where needed)
                vm = new ManualQuoteViewModel
                {
                    Id = data.Id,
                    QuoteNo = data.QuoteNo,
                    RevisionID = data.RevisionID,
                    ImportDate = data.ImportDate,
                    QuantityOfUnits = data.QuantityOfUnits,
                    DueDate = data.DueDate,
                    DesignerId = data.DesignerId?.ToString(),
                    ImportedPMRJBFileRef = data.ImportedPMRJBFileRef,

                    SiteName = data.SiteName,
                    StandNumber = data.StandNumber,
                    ProjectName = data.ProjectName,
                    UnitBlockNumber = data.UnitBlockNumber,
                    UnitBlockType = data.UnitBlockType,
                    StreetAddress = data.StreetAddress,

                    PotentialCustomerId = data.PotentialCustomerId,
                    SalesRepId = data.SalesRepId?.ToString(),

                    SelectedContactId = data.ContactId,
                    SelectedEnquiryId = data.EnquiryId,

                    ShipToStreet1 = data.ShipToStreet1,
                    ShipToStreet2 = data.ShipToStreet2,
                    ShipToStreet3 = data.ShipToStreet3,
                    ShipToCity = data.ShipToCity,
                    ShipToState = data.ShipToState,
                    ShipToPostalCode = data.ShipToPostalCode,
                    ShipToCountry = data.ShipToCountry,

                    RoofPitch = data.RoofPitch,
                    RoofOverhang = data.RoofOverhang,
                    RoofGableOverhang = data.RoofGableOverhang,
                    RoofCovering = data.RoofCovering,
                    MaxBattenCenters = data.MaxBattenCenters,
                    MaxTrustCenters = data.MaxTrustCenters,
                    FloorArea = data.FloorArea,
                    RoofArea = data.RoofArea,

                    QuoteNotes = data.QuoteNotes,
                    SalesNotes = data.SalesNotes,

                    PotentialCustomerName = data.PotentialCustomerName,
                    SelectedContactName = data.SelectedContactName,
                    SelectedEnquiryNumber = data.SelectedEnquiryNumber,

                    // if you show the badge:
                    QuoteImageCount = db.QuoteImages.Count(x => x.QuoteId == data.Id)
                };

                PopulateLookups(vm);

                // 3) Line items in a separate, trimmed query
                vm.LineItems = db.ManualQuoteLineItems
                    .AsNoTracking()
                    .Where(li => li.ManualQuoteId == vm.Id)
                    .Select(li => new QuoteLineItemViewModel
                    {
                        ProductId = li.ProductId,
                        ProductCode = li.Product.ProductCode,   // EF will LEFT JOIN Product just for these columns
                        ProductDescription = li.Product.Name,
                        BaseUOM = li.Product.BaseUOM,
                        Qty = li.Qty,
                        CostPrice = li.CostPrice,
                        MarginPercent = li.MarginPercent,
                        SellingPrice = li.SellingPrice
                    })
                    .ToList();

                vm.LineItemsJson = JsonConvert.SerializeObject(vm.LineItems);
            }


            return View(vm);
        }

        private void PopulateLookups(ManualQuoteViewModel vm)
        {
            vm.DesignerOptions = db.AppUsers
                .AsNoTracking()
                .Where(u => u.IsActive && u.IsDesigner)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToList();

            vm.Customers = db.Customers
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Select(c => new CustomerDropdownItem { Id = c.Id, CustomerName = c.CustomerName })
                .OrderBy(c => c.CustomerName)
                .ToList();

            vm.SalesRepOptions = db.AppUsers
                .AsNoTracking()
                .Where(u => u.IsActive && u.IsSalesRep)
                .OrderBy(u => u.FullName)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.FullName })
                .ToList();

            vm.Enquiries = db.Enquiries
                .Include(e => e.Customer)
                .OrderByDescending(e => e.EnquiryNumber) // or whatever
                .Select(e => new EnquiryDropdownItem
                {
                    EnquiryId = e.EnquiryId,
                    EnquiryNumber = e.EnquiryNumber,
                    CustomerName = e.Customer.CustomerName
                })
                .ToList();
        }

        [HttpPost]
        public JsonResult AddEnquiry(string EnquiryNumber, int CustomerId, string Notes)
        {
            var newEnquiry = new Enquiry
            {
                EnquiryNumber = EnquiryNumber,
                CustomerId = CustomerId,
                Notes = Notes,
                Status = "Open",
                CreatedAt = DateTime.Now,
                CreatedByUserId = 1,
                SalesRepUserId = 1
            };

            db.Enquiries.Add(newEnquiry);
            try
            {
                db.SaveChanges();
            }
            catch (Exception x)
            {
                System.Diagnostics.Debug.WriteLine(x.ToString());
                throw; // optional: rethrow to see full error trace in browser
            }

            return Json(new { newEnquiry.EnquiryId, newEnquiry.EnquiryNumber });
        }

        // POST: ManualQuote/SaveManualQuote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveManualQuote(ManualQuoteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Customers = db.Customers
                    .Where(c => c.IsActive)
                    .Select(c => new CustomerDropdownItem
                    {
                        Id = c.Id,
                        CustomerName = c.CustomerName
                    })
                    .ToList();

                model.DesignerOptions = db.AppUsers
                    .Where(u => u.IsActive && u.IsDesigner)
                    .OrderBy(u => u.FullName)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.FullName
                    })
                    .ToList();


                return View("ManualQuote", model);
            }

            var quote = new ManualQuote
            {
                QuoteNo = model.QuoteNo,
                RevisionID = model.RevisionID,
                ImportDate = model.ImportDate,
                QuantityOfUnits = model.QuantityOfUnits,
                DueDate = model.DueDate,
                DesignerId = string.IsNullOrWhiteSpace(model.DesignerId) ? (int?)null : int.Parse(model.DesignerId),
                ImportedPMRJBFileRef = model.ImportedPMRJBFileRef,
                SiteName = model.SiteName,
                StandNumber = model.StandNumber,
                ProjectName = model.ProjectName,
                UnitBlockNumber = model.UnitBlockNumber,
                UnitBlockType = model.UnitBlockType,
                StreetAddress = model.StreetAddress,
                PotentialCustomerId = model.PotentialCustomerId,
                EnquiryId = model.SelectedEnquiryId,
                SalesRepId = string.IsNullOrWhiteSpace(model.SalesRepId) ? (int?)null : int.Parse(model.SalesRepId),
                ContactId = model.SelectedContactId,
                ShipToStreet1 = model.ShipToStreet1,
                ShipToStreet2 = model.ShipToStreet2,
                ShipToStreet3 = model.ShipToStreet3,
                ShipToCity = model.ShipToCity,
                ShipToState = model.ShipToState,
                ShipToPostalCode = model.ShipToPostalCode,
                ShipToCountry = model.ShipToCountry,
                RoofPitch = model.RoofPitch,
                RoofOverhang = model.RoofOverhang,
                RoofGableOverhang = model.RoofGableOverhang,
                RoofCovering = model.RoofCovering,
                MaxBattenCenters = model.MaxBattenCenters,
                MaxTrustCenters = model.MaxTrustCenters,
                FloorArea = model.FloorArea,
                RoofArea = model.RoofArea,
                QuoteNotes = model.QuoteNotes,
                SalesNotes = model.SalesNotes,
                CreatedAt = DateTime.Now
            };

            // Auto-generate QuoteNo and RevisionID
            if (string.IsNullOrWhiteSpace(model.QuoteNo))
            {
                var yearSuffix = DateTime.Now.Year % 100;
                var prefix = $"Q{yearSuffix:D2}";

                var lastNumber = db.ManualQuotes
                    .Where(q => q.QuoteNo.StartsWith(prefix))
                    .Select(q => q.QuoteNo.Substring(3, 4)) // Extract the 4-digit numeric sequence
                    .ToList()
                    .Select(s => int.TryParse(s, out var n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                model.QuoteNo = $"{prefix}{(lastNumber + 1):D4}";
                model.RevisionID = "A"; // First revision
            }
            else
            {
                // Existing QuoteNo — generate next revision letter
                var existingRevisions = db.ManualQuotes
                    .Where(q => q.QuoteNo == model.QuoteNo)
                    .Select(q => q.RevisionID)
                    .ToList();

                char nextRev = 'A';

                if (existingRevisions.Any())
                {
                    var lastChar = existingRevisions
                        .Where(r => !string.IsNullOrWhiteSpace(r) && r.Length == 1 && char.IsLetter(r[0]))
                        .Select(r => r[0])
                        .OrderByDescending(c => c)
                        .FirstOrDefault();

                    nextRev = (char)(lastChar + 1);
                }

                model.RevisionID = nextRev.ToString();
            }


            // Assign and save
            quote.QuoteNo = model.QuoteNo;
            quote.RevisionID = model.RevisionID;

            if (quote.Id == 0)
            {
                quote.QuoteStage = "Qualify"; // start here
            }
            db.ManualQuotes.Add(quote);
            db.SaveChanges();

            // Deserialize JSON line items into model.LineItems
            if (!string.IsNullOrEmpty(model.LineItemsJson))
            {
                model.LineItems = JsonConvert.DeserializeObject<List<QuoteLineItemViewModel>>(model.LineItemsJson);
            }

            // Now add line items
            if (model.LineItems != null && model.LineItems.Any())
            {
                foreach (var item in model.LineItems)
                {
                    var lineItem = new ManualQuoteLineItem
                    {
                        ManualQuoteId = quote.Id,
                        ProductId = item.ProductId,
                        Qty = item.Qty,
                        CostPrice = item.CostPrice,
                        MarginPercent = item.MarginPercent,
                        SellingPrice = item.SellingPrice
                    };
                    db.ManualQuoteLineItems.Add(lineItem);
                }
                db.SaveChanges();
            }

            TempData["Message"] = "Quote saved successfully!";

            // Check if we came from the tabs view
            bool returnToTabs = Request["ReturnToTabs"] == "true";
            int savedId = quote.Id;
            return RedirectToAction(
                returnToTabs ? "ManualQuote_Tabs" : "ManualQuote",
                new { id = savedId }
            );
        }

        // Contact Modal
        public ActionResult GetContacts(int customerId)
        {
            var contacts = db.Contacts
                .Where(c => c.CustomerId == customerId)
                .Select(c => new ContactDropdownItem
                {
                    ContactId = c.ContactId,
                    Name = c.Name,
                    Designation = c.Designation,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    CustomerId = c.CustomerId
                })
                .ToList();

            return PartialView("_ContactSearchModal", contacts);
        }

        // Stock Items for DataTables
        [HttpPost]
        public JsonResult GetFilteredStockItems()
        {
            var draw = Request.Form["draw"];
            var start = Convert.ToInt32(Request.Form["start"]);
            var length = Convert.ToInt32(Request.Form["length"]);
            var searchValue = Request.Form["search[value]"];

            var query = db.Products.Where(p => p.IsActive && p.ItemType != null);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(p =>
                    p.ProductCode.Contains(searchValue) ||
                    p.Name.Contains(searchValue) ||
                    p.ItemType.Name.Contains(searchValue));
            }

            int filteredCount = query.Count();

            var data = query
                .OrderBy(p => p.ProductCode)
                .Skip(start)
                .Take(length)
                .Select(p => new
                {
                    productId = p.Id,
                    productCode = p.ProductCode,
                    name = p.Name,
                    itemTypeName = p.ItemType.Name,
                    baseUOM = p.BaseUOM
                })
                .ToList();

            return Json(new
            {
                draw = draw,
                recordsFiltered = filteredCount,
                recordsTotal = db.Products.Count(),
                data = data
            });
        }

        public ActionResult ListManualQuotes()
        {
            var quotes = db.ManualQuotes
                .Include("SalesRep")
                .Include("PotentialCustomer")
                .OrderByDescending(q => q.ImportDate)
                .ToList();

            return View(quotes);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //PDF Preview & Generate
        [HttpPost]
        public ActionResult GeneratePdfQuote(ManualQuoteViewModel model)
        {
            if (model == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Quote model is null");

            try
            {
                // Generate QuoteNo and RevisionID if needed (same logic from SaveManualQuote)
                if (string.IsNullOrWhiteSpace(model.QuoteNo))
                {
                    var yearSuffix = DateTime.Now.Year % 100;
                    var prefix = $"Q{yearSuffix:D2}";

                    var lastNumber = db.ManualQuotes
                        .Where(q => q.QuoteNo.StartsWith(prefix))
                        .Select(q => q.QuoteNo.Substring(3, 4))
                        .ToList()
                        .Select(s => int.TryParse(s, out var n) ? n : 0)
                        .DefaultIfEmpty(0)
                        .Max();

                    model.QuoteNo = $"{prefix}{(lastNumber + 1):D4}";
                    model.RevisionID = "A";
                }
                else
                {
                    var existingRevisions = db.ManualQuotes
                        .Where(q => q.QuoteNo == model.QuoteNo)
                        .Select(q => q.RevisionID)
                        .ToList();

                    char nextRev = 'A';

                    if (existingRevisions.Any())
                    {
                        var lastChar = existingRevisions
                            .Where(r => !string.IsNullOrWhiteSpace(r) && r.Length == 1 && char.IsLetter(r[0]))
                            .Select(r => r[0])
                            .OrderByDescending(c => c)
                            .FirstOrDefault();

                        nextRev = (char)(lastChar + 1);
                    }

                    model.RevisionID = nextRev.ToString();
                }

                var quote = new ManualQuote
                {
                    QuoteNo = model.QuoteNo,
                    RevisionID = model.RevisionID,
                    ImportDate = model.ImportDate,
                    QuantityOfUnits = model.QuantityOfUnits,
                    DueDate = model.DueDate,
                    DesignerId = string.IsNullOrWhiteSpace(model.DesignerId) ? (int?)null : int.Parse(model.DesignerId),
                    ImportedPMRJBFileRef = model.ImportedPMRJBFileRef,
                    SiteName = model.SiteName,
                    StandNumber = model.StandNumber,
                    ProjectName = model.ProjectName,
                    UnitBlockNumber = model.UnitBlockNumber,
                    UnitBlockType = model.UnitBlockType,
                    StreetAddress = model.StreetAddress,
                    PotentialCustomerId = model.PotentialCustomerId,
                    SalesRepId = string.IsNullOrWhiteSpace(model.SalesRepId) ? (int?)null : int.Parse(model.SalesRepId),
                    ContactId = model.SelectedContactId,
                    ShipToStreet1 = model.ShipToStreet1,
                    ShipToStreet2 = model.ShipToStreet2,
                    ShipToStreet3 = model.ShipToStreet3,
                    ShipToCity = model.ShipToCity,
                    ShipToState = model.ShipToState,
                    ShipToPostalCode = model.ShipToPostalCode,
                    ShipToCountry = model.ShipToCountry,
                    RoofPitch = model.RoofPitch,
                    RoofOverhang = model.RoofOverhang,
                    RoofGableOverhang = model.RoofGableOverhang,
                    RoofCovering = model.RoofCovering,
                    MaxBattenCenters = model.MaxBattenCenters,
                    MaxTrustCenters = model.MaxTrustCenters,
                    FloorArea = model.FloorArea,
                    RoofArea = model.RoofArea,
                    QuoteNotes = model.QuoteNotes,
                    SalesNotes = model.SalesNotes,
                    CreatedAt = DateTime.Now
                };

                db.ManualQuotes.Add(quote);
                db.SaveChanges();

                if (!string.IsNullOrEmpty(model.LineItemsJson))
                {
                    model.LineItems = JsonConvert.DeserializeObject<List<QuoteLineItemViewModel>>(model.LineItemsJson);
                }

                if (model.LineItems != null && model.LineItems.Any())
                {
                    foreach (var item in model.LineItems)
                    {
                        var lineItem = new ManualQuoteLineItem
                        {
                            ManualQuoteId = quote.Id,
                            ProductId = item.ProductId,
                            Qty = item.Qty,
                            CostPrice = item.CostPrice,
                            MarginPercent = item.MarginPercent,
                            SellingPrice = item.SellingPrice
                        };
                        db.ManualQuoteLineItems.Add(lineItem);
                    }
                    db.SaveChanges();
                }

                var outputDir = Server.MapPath("~/PdfTemplates/GeneratedQuotes");
                var generator = new QuotePdfGenerator();
                string pdfPath = generator.GenerateManualQuotePdf(quote.Id, outputDir, quote.QuoteNo, quote.RevisionID);

                // Extract just the filename
                var fileName = Path.GetFileName(pdfPath);

                // Build the relative path to store in DB
                quote.GeneratedPdfPath = Url.Content("~/PdfTemplates/GeneratedQuotes/" + fileName);

                // Save to DB
                db.Entry(quote).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, path = pdfPath });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public ActionResult RenderPreview(ManualQuoteViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Model is null");
            }

            try
            {
                // Enrich with Sales Rep / Designer names
                if (int.TryParse(model.SalesRepId, out int salesRepId))
                {
                    var salesRep = db.AppUsers.FirstOrDefault(u => u.Id == salesRepId);
                    model.SelectedSalesRepName = salesRep?.FullName ?? "-";
                }

                if (int.TryParse(model.DesignerId, out int designerId))
                {
                    var designer = db.AppUsers.FirstOrDefault(u => u.Id == designerId);
                    model.SelectedDesignerName = designer?.FullName ?? "-";
                }

                // Deserialize LineItems if needed
                if (!string.IsNullOrEmpty(model.LineItemsJson) && (model.LineItems == null || !model.LineItems.Any()))
                {
                    model.LineItems = JsonConvert.DeserializeObject<List<QuoteLineItemViewModel>>(model.LineItemsJson);
                }

                // Render the full view for preview
                return View("QuotePdfPreview", model);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Render failed: " + ex.Message);
            }
        }
    }
}
