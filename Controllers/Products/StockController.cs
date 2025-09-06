
using MillenniumWebFixed.Hubs;
using MillenniumWebFixed.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class StockController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        [HttpPost]
        public JsonResult ValidateExcel(HttpPostedFileBase file)
        {
            var result = new
            {
                valid = false,
                message = "",
                preview = new List<Dictionary<string, string>>()
            };

            if (file == null || file.ContentLength == 0)
            {
                return Json(new { valid = false, message = "No file uploaded." });
            }

            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("Private Imports");

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var sheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (sheet == null)
                        return Json(new { valid = false, message = "No worksheet found in the Excel file." });

                    // Get header row
                    var headers = new List<string>();
                    for (int col = 1; col <= sheet.Dimension.End.Column; col++)
                    {
                        var headerText = sheet.Cells[1, col].Text?.Trim();
                        if (!string.IsNullOrWhiteSpace(headerText))
                            headers.Add(headerText);
                    }

                    // Define your required headers here
                    var requiredHeaders = new[] { "Product Code", "Description", "Material Type", "Class" };

                    var missing = requiredHeaders.Where(h => !headers.Contains(h)).ToList();
                    if (missing.Any())
                    {
                        return Json(new
                        {
                            valid = false,
                            message = "Missing required columns: " + string.Join(", ", missing)
                        });
                    }

                    // Extract up to 10 preview rows
                    var preview = new List<Dictionary<string, string>>();
                    for (int row = 2; row <= Math.Min(sheet.Dimension.End.Row, 11); row++)
                    {
                        var rowData = new Dictionary<string, string>();
                        for (int col = 1; col <= headers.Count; col++)
                        {
                            string colName = headers[col - 1];
                            string cellValue = sheet.Cells[row, col].Text?.Trim();
                            rowData[colName] = cellValue;
                        }
                        preview.Add(rowData);
                    }

                    return Json(new { valid = true, preview });
                }
            }
            catch (Exception ex)
            {
                return Json(new { valid = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetProductDetails(int id)
        {
            //var product = db.Products
            //    .Include(p => p.Properties)
            //    .Include(p => p.Assemblies.Select(a => a.Components.Select(c => c.ComponentProduct)))
            //    .FirstOrDefault(p => p.Id == id);
            var product = db.Products
                    .Include("Properties")
                    .Include("Assemblies.Components.ComponentProduct.ItemType")
                    .FirstOrDefault(p => p.Id == id);

            var result = new
            {
                properties = product.Properties.Select(p => new
                {
                    p.Id,
                    p.PropertyName,
                    p.Value,
                    p.Units
                }),
                assemblies = product.Assemblies.Select(a => new
                {
                    id = a.Id,
                    name = a.Name,  // Explicitly map this way if in doubt
                    components = a.Components.Select(c => new
                    {
                        id = c.Id,
                        qtyPerUOM = c.QtyPerUOM,
                        unit = c.Unit,
                        costPerUnit = c.CostPerUnit,
                        totalFormula = c.TotalFormula,
                        notes = c.Notes,
                        componentProduct = new
                        {
                            name = c.ComponentProduct.Name,
                            itemType = c.ComponentProduct.ItemType.Name
                        }
                    })
                })

            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ImportStockJson()
        {
            bool hasStock = db.Products.Any();
            ViewBag.HasStock = hasStock;
            return View();
        }

        public ActionResult List()
        {
            var items = db.StockItems.Take(10).ToList();
            return View("ListStock", items);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Import()
        {
            bool hasStock = db.StockItems.Any(); // adjust if needed

            ViewBag.HasStock = hasStock;
            return View("ImportStock");
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                ExcelPackage.License.SetNonCommercialOrganization("Private Imports");

                using (var package = new ExcelPackage(file.InputStream))
                {
                    var sheet = package.Workbook.Worksheets[0];
                    var rowCount = sheet.Dimension.Rows;
                    var items = new List<StockItem>();
                    int totalRows = rowCount - 1;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        int current = row - 1;
                        double percent = (current / (double)totalRows) * 100;
                        items.Add(new StockItem
                        {
                            ProductCode = sheet.Cells[row, 1].Text,
                            Description = sheet.Cells[row, 2].Text,
                            Units = sheet.Cells[row, 3].Text,
                            WeightKg = TryParseDouble(sheet.Cells[row, 4].Text),
                            ThicknessMM = TryParseDouble(sheet.Cells[row, 5].Text),
                            WidthMM = TryParseDouble(sheet.Cells[row, 6].Text),
                            LengthMM = TryParseDouble(sheet.Cells[row, 7].Text),
                            DiameterMM = TryParseDouble(sheet.Cells[row, 8].Text),
                            HeightDepthMM = TryParseDouble(sheet.Cells[row, 9].Text),
                            AreaM2 = TryParseDouble(sheet.Cells[row, 10].Text),
                            SurfaceAreaM2 = TryParseDouble(sheet.Cells[row, 11].Text),
                            VolumeM3 = TryParseDouble(sheet.Cells[row, 12].Text),
                            MaterialType = sheet.Cells[row, 13].Text,
                            VisualGrade = sheet.Cells[row, 14].Text,
                            StrengthGrade = sheet.Cells[row, 15].Text,
                            Class = sheet.Cells[row, 16].Text,
                            Colour = sheet.Cells[row, 17].Text,
                            Manufacturer = sheet.Cells[row, 18].Text,
                            Notes = sheet.Cells[row, 19].Text,
                            WasEnriched = sheet.Cells[row, 20].Text,
                            ImageUrl = sheet.Cells[row, 21].Text,
                            InstallGuideUrl = sheet.Cells[row, 22].Text,
                            ScrapedSpecs = sheet.Cells[row, 23].Text
                        });
                    }

                    const int batchSize = 10;
                    int total = items.Count;
                    int batches = (int)Math.Ceiling(total / (double)batchSize);

                    for (int i = 0; i < batches; i++)
                    {
                        var batch = items.Skip(i * batchSize).Take(batchSize).ToList();
                        db.StockItems.AddRange(batch);
                        db.SaveChanges();

                        int saved = Math.Min((i + 1) * batchSize, total);
                        double percent = (saved / (double)total) * 100;
                        ProgressHub.Send($"Saving batch (size {batchSize}) {i + 1} of {batches} ({percent:F0}%)");
                    }

                    ProgressHub.Send("✅ Stock import complete.");
                    TempData["Message"] = "Stock imported successfully.";
                    return RedirectToAction("Index");
                }
            }

            TempData["Error"] = "Please upload a valid file.";
            return RedirectToAction("Import");
        }

        private double? TryParseDouble(string input)
        {
            return double.TryParse(input, out double result) ? (double?)result : null;
        }

        public ActionResult ListStockJson()
        {
            var products = db.Products
                .Include("ItemType.UOMConversions")
                .Include("ItemType.Properties")
                .Include("Properties")
                .Include("Assemblies.Components")
                .Include("Assemblies.Components.ComponentProduct")
                .ToList();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportStockJson(HttpPostedFileBase jsonFile)
        {
            if (jsonFile == null || jsonFile.ContentLength == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction("ImportStockJson");
            }

            var products = new List<ProductImportModel>();

            try
            {
                // Clean Data --> Stock (Product Tables)
                ProgressHub.Send($"Cleaning old stock / product data...");
                db.Database.ExecuteSqlCommand("DELETE FROM prod_ProductProperty");
                db.Database.ExecuteSqlCommand("DELETE FROM prod_ProductAssemblyComponent");  // has FK to Products
                db.Database.ExecuteSqlCommand("DELETE FROM prod_ProductAssembly");           // has FK to Products
                db.Database.ExecuteSqlCommand("DELETE FROM prod_Product");                   // finally safe to delete

                ExcelPackage.License.SetNonCommercialOrganization("Private Imports");

                using (var package = new ExcelPackage(jsonFile.InputStream))
                {
                    var worksheet = package.Workbook.Worksheets.First();
                    int rowCount = worksheet.Dimension.End.Row;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var product = new ProductImportModel
                        {
                            Name = worksheet.Cells[row, 2].Text,
                            ProductCode = worksheet.Cells[row, 1].Text,
                            ItemType = "Anchor Bracket",
                            BaseUOM = worksheet.Cells[row, 3].Text,
                            IsActive = true,
                            Properties = new Dictionary<string, string>
                    {
                        { "Width", worksheet.Cells[row, 6].Text },
                        { "Thickness", worksheet.Cells[row, 5].Text },
                        { "Length", worksheet.Cells[row, 8].Text },
                        { "Depth", worksheet.Cells[row, 10].Text },
                        { "Grade", worksheet.Cells[row, 17].Text },
                        { "Manufacturer", worksheet.Cells[row, 19].Text }
                    }
                        };
                        products.Add(product);
                    }
                }

                db.Configuration.AutoDetectChangesEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int index = 0;
                int batchSize = 100;
                int total = products.Count;
                ProgressHub.Send($"Starting import, batches of {batchSize}...");

                foreach (var item in products)
                {
                    index++;
                    if (index % 100 == 0 || index == total)
                    {
                        ProgressHub.Send($"Imported {index:N0} of {total:N0}...");
                    }

                    // Cache ItemType for performance
                    var itemType = db.ItemTypes.Local.FirstOrDefault(x => x.Name == item.ItemType)
                                   ?? db.ItemTypes.FirstOrDefault(x => x.Name == item.ItemType);

                    if (itemType == null)
                    {
                        itemType = new ItemType
                        {
                            Name = item.ItemType,
                            BaseUOM = item.BaseUOM ?? "ea",
                            SupportsM2 = false,
                            SupportsM3 = false,
                            Notes = "Auto-created from stock import"
                        };
                        db.ItemTypes.Add(itemType);
                        db.SaveChanges(); // This needs to happen so we get the ID
                    }

                    var product = new Product
                    {
                        Name = item.Name,
                        ProductCode = item.ProductCode,
                        ItemTypeId = itemType.Id,
                        BaseUOM = item.BaseUOM,
                        IsActive = item.IsActive
                    };

                    db.Products.Add(product);
                    db.SaveChanges(); // Required immediately to get product.Id

                    foreach (var prop in item.Properties)
                    {
                        if (!string.IsNullOrWhiteSpace(prop.Value))
                        {
                            db.ProductProperties.Add(new ProductProperty
                            {
                                ProductId = product.Id,
                                PropertyName = prop.Key,
                                Value = prop.Value,
                                Units = ""
                            });
                        }
                    }

                    // Save in batches
                    if (index % batchSize == 0)
                    {
                        db.SaveChanges();
                        db.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);
                    }
                }

                db.SaveChanges(); // Final flush
                db.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);

                db.Configuration.AutoDetectChangesEnabled = true;
                db.Configuration.ValidateOnSaveEnabled = true;

                TempData["Message"] = "Stock imported successfully.";
            }
            catch (Exception ex)
            {
                try
                {
                    db.ImportFailures.Add(new ImportFailure
                    {
                        ProjectId = "0",
                        Section = "Stock",
                        ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                        TimeStamp = DateTime.Now
                    });
                    db.SaveChanges();
                }
                catch (Exception logEx)
                {
                    System.Diagnostics.Debug.WriteLine("Logging failure: " + logEx.Message);
                }

                TempData["Error"] = "Import error: " + ex.Message + "<br/>" + ex.InnerException;
            }

            return RedirectToAction("ImportStockJson");
        }

        public ActionResult ManageInventory(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateInventory(Product model)
        {
            var product = db.Products.Find(model.Id);
            if (product == null)
                return HttpNotFound();

            product.StockLevel = model.StockLevel;
            product.ReorderPoint = model.ReorderPoint;
            product.LeadTimeDays = model.LeadTimeDays;
            product.CostPrice = model.CostPrice;
            product.SellingPrice = model.SellingPrice;
            product.Notes = model.Notes;

            db.SaveChanges();
            TempData["Message"] = "Inventory updated.";
            return RedirectToAction("ListStockJson");
        }

        public class ProductImportModel
        {
            public string Name { get; set; }
            public string ProductCode { get; set; }
            public string ItemType { get; set; }
            public string BaseUOM { get; set; }
            public bool IsActive { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }

        public class ProductWithConversionsViewModel
        {
            public Product Product { get; set; }
            public List<UOMConversion> Conversions { get; set; }
        }
    }
}
