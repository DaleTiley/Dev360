using MillenniumWebFixed.Controllers.Projects;
using MillenniumWebFixed.Hubs;
using MillenniumWebFixed.Models;
using MillenniumWebFixed.Services;
using MillenniumWebFixed.ViewModels;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ImportController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();
        private readonly ExcelImporter importer;

        public ImportController()
        {
            importer = new ExcelImporter(db);
        }

        //Excel Revamped
        public ActionResult ImportExcel()
        {
            return View();
        }

        //NEW EXCEL IMPORT
        [HttpPost]
        public ActionResult UploadExcel(HttpPostedFileBase excelFile)
        {
            if (excelFile == null || excelFile.ContentLength == 0 ||
                !(excelFile.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                  excelFile.FileName.EndsWith(".xlsm", StringComparison.OrdinalIgnoreCase) ||
                  excelFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)))
            {
                TempData["Error"] = "Please upload a valid Excel (.xlsx/.xlsm) or CSV file.";
                return RedirectToAction("ImportExcel");
            }

            ExcelPackage.License.SetNonCommercialOrganization("MR DEV");

            // Read the upload into memory so we can hash + parse without stream headaches
            using (var ms = new MemoryStream())
            {
                excelFile.InputStream.CopyTo(ms);
                ms.Position = 0;

                string fileHash = ComputeSha256(ms);
                ms.Position = 0;

                try
                {
                    using (var package = new ExcelPackage(ms))
                    using (var db = new AppDbContext())
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["Error"] = "No worksheet found.";
                            return RedirectToAction("ImportExcel");
                        }

                        // ----------------------
                        // 1) Parse top sheet → fill your GeneralProjectData (as before)
                        // ----------------------
                        int rowCount = worksheet.Dimension?.Rows ?? 0;

                        var project = new GeneralProjectData();
                        string currentSection = null;

                        var labelMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // (your mapping unchanged) …
                    { "Designer", "Designer" },
                    { "Customer (client)|Name", "CustomerName" },
                    { "Customer (client)|Address", "CustomerAddress" },
                    { "Customer (client)|ID", "CustomerId" },
                    { "Customer (client)|Data path", "CustomerDataPath" },
                    { "Final client (future house owner)", "FinalClientName" },
                    { "Site Location", "SiteLocation" },
                    { "Site Address", "SiteAddress" },
                    { "Project name", "ProjectName" },
                    { "Fabricator", "Fabricator" },
                    { "Template", "Template" },
                    { "Project type", "ProjectType" },
                    { "Dead load (N/m²)", "DeadLoadRoof" },
                    { "Snow zone", "SnowZone" },
                    { "Altitude", "Altitude" },
                    { "Wind zone (region)", "WindZone" },
                    { "Pitch", "Pitch" },
                    { "Frame spacing", "FrameSpacing" },
                    { "Heel type", "HeelType" },
                    { "VAT", "VATPercentage" },
                    // sub-sections …
                    { "Vertical heel|Offset", "VerticalHeelOffset" },
                    { "Vertical heel|Vertical height", "VerticalHeelHeight" },
                    { "Standard heel|Offset", "StandardHeelOffset" },
                    { "Standard heel|Perpendicular height", "StandardHeelHeight" },
                    { "French perpendicular|Offset", "FrenchPerpendicularOffset" },
                    { "French perpendicular|Perpendicular height", "FrenchPerpendicularHeight" },
                    { "Extended bottom chord|Offset", "ExtendedBottomChordOffset" },
                    { "Extended bottom chord|Vertical height", "ExtendedBottomChordHeight" },
                    { "Wall plate|Width", "WallPlateWidth" },
                    { "Wall plate|Layback from outside", "WallPlateLayback" },
                    { "Total material volume|Timber", "TotalMaterialVolumeTimber" },
                    { "Total material volume|Circular timber", "TotalMaterialVolumeCircularTimber" },
                    { "Total material volume|LVL", "TotalMaterialVolumeLVL" },
                    { "Total material volume|Glulam", "TotalMaterialVolumeGlulam" },
                    { "Total material price|Timber", "TotalMaterialPriceTimber" },
                    { "Total material price|LVL", "TotalMaterialPriceLVL" },
                    { "Total material price|Glulam", "TotalMaterialPriceGlulam" },
                    { "Total material price|All wooden items", "TotalMaterialPriceAllWoodenItems" },
                    { "Total material price|Plates", "TotalMaterialPricePlates" },
                    { "Total material price|Posi-Struts", "TotalMaterialPricePosiStruts" }
                };

                        for (int row = 1; row <= rowCount; row++)
                        {
                            var col1 = worksheet.Cells[row, 1].Text.Trim();
                            var col2 = worksheet.Cells[row, 2].Text.Trim();
                            var col3 = worksheet.Cells[row, 3].Text.Trim();

                            if (!string.IsNullOrEmpty(col1) && !string.IsNullOrEmpty(col2) && !string.IsNullOrEmpty(col3))
                            {
                                currentSection = col1;
                                var compositeKey = $"{currentSection}|{col2}";
                                if (labelMap.TryGetValue(compositeKey, out string prop)) SetProperty(project, prop, col3);
                            }
                            else if (!string.IsNullOrEmpty(col1) && string.IsNullOrEmpty(col2) && !string.IsNullOrEmpty(col3))
                            {
                                if (labelMap.TryGetValue(col1, out string prop)) SetProperty(project, prop, col3);
                            }
                            else if (!string.IsNullOrEmpty(col1) && string.IsNullOrEmpty(col2) && string.IsNullOrEmpty(col3))
                            {
                                currentSection = col1;
                            }
                            else if (string.IsNullOrEmpty(col1) && !string.IsNullOrEmpty(col2) && !string.IsNullOrEmpty(col3))
                            {
                                var compositeKey = $"{currentSection}|{col2}";
                                if (labelMap.TryGetValue(compositeKey, out string prop)) SetProperty(project, prop, col3);
                            }
                        }

                        // Guard: we need ProjectName at minimum
                        var projectCode = (project.ProjectName ?? "").Trim();
                        if (string.IsNullOrEmpty(projectCode))
                        {
                            TempData["Error"] = "Project name not found in the Excel header.";
                            return RedirectToAction("ImportExcel");
                        }

                        // ----------------------
                        // 2) Ensure hub: Project + next Version
                        // ----------------------
                        // prefer Address; fallback to Location so SiteName isn't null
                        var siteForHub = !string.IsNullOrWhiteSpace(project.SiteAddress)
                            ? project.SiteAddress
                            : project.SiteLocation;

                        // NEW: filename + user
                        var originalName = Path.GetFileName(excelFile.FileName);
                        var importedBy = User?.Identity?.Name ?? Environment.UserName;

                        // Ensure hub: Project + next Version (now with filename & user)
                        var (hubProject, hubVersion, duplicateInfo) =
                            EnsureProjectAndNextVersion(
                                db,
                                projectCode,
                                project.CustomerName,
                                siteForHub,
                                fileHash,
                                originalName,       // <-- fills ProjectImportBatch.OriginalFileName
                                importedBy          // <-- fills ProjectImportBatch.ImportedBy
                            );

                        if (duplicateInfo != null)
                        {
                            TempData["Info"] = duplicateInfo;
                            return RedirectToAction("ImportExcel");
                        }


                        // Stamp hub FKs + keep your legacy Version field in GPD
                        project.ProjectId = hubProject.Id;     // ✅ set FK, DO NOT set project.Id
                        project.Version = hubVersion.VersionNo;
                        project.CreatedAt = DateTime.Now;

                        db.GeneralProjectDatas.Add(project);
                        db.SaveChanges();

                        ProgressHub.Send("Main General Data Saved, starting sections...");

                        var gpdId = project.Id;                // identity assigned by the DB after SaveChanges


                        // ----------------------
                        // 3) Import the remaining sheets (pass context with ALL IDs)
                        // ----------------------
                        var ctx = new ImportContext
                        {
                            GeneralProjectDataId = gpdId,
                            ProjectId = hubProject.Id,
                            ProjectVersionId = hubVersion.Id
                        };

                        int projectId = project.Id;            // same as before (GeneralProjectData.Id)
                        int totalSections = 20, completed = 0;
                        for (int i = 1; i < package.Workbook.Worksheets.Count; i++) // skip first (General Data)
                        {
                            var sheet = package.Workbook.Worksheets[i];
                            var sheetName = sheet.Name.Trim().Replace(" ", "").Replace("-", "_");
                            if (sheet.Dimension == null || sheet.Dimension.End.Row <= 1) continue;

                            switch (sheetName.ToLowerInvariant())
                            {
                                case "framingzones": completed++; ProgressHub.Send($"Importing Framing Zones: {(completed * 100) / totalSections}%"); importer.Importxls_FramingZones(sheet, ctx); break;
                                case "frames": completed++; ProgressHub.Send($"Importing Frames: {(completed * 100) / totalSections}%"); importer.Importxls_Frames(sheet, ctx); break;
                                case "manufactureframes": completed++; ProgressHub.Send($"Importing Manufacture Frames: {(completed * 100) / totalSections}%"); importer.Importxls_Manufactureframes(sheet, ctx); break;
                                case "timber": completed++; ProgressHub.Send($"Importing Timber: {(completed * 100) / totalSections}%"); importer.Importxls_Timber(sheet, ctx); break;
                                case "cladding": completed++; ProgressHub.Send($"Importing Cladding: {(completed * 100) / totalSections}%"); importer.Importxls_Cladding(sheet, ctx); break;
                                case "boards": completed++; ProgressHub.Send($"Importing Boards: {(completed * 100) / totalSections}%"); importer.Importxls_Boards(sheet, ctx); break;
                                case "connectorplates": completed++; ProgressHub.Send($"Importing Connector Plates: {(completed * 100) / totalSections}%"); importer.Importxls_ConnectorPlates(sheet, ctx); break;
                                case "posistruts": completed++; ProgressHub.Send($"Importing Posi Struts: {(completed * 100) / totalSections}%"); importer.Importxls_PosiStruts(sheet, ctx); break;
                                case "metalwork": completed++; ProgressHub.Send($"Importing Metalwork: {(completed * 100) / totalSections}%"); importer.Importxls_Metalwork(sheet, ctx); break;
                                case "bracing": completed++; ProgressHub.Send($"Importing Bracing: {(completed * 100) / totalSections}%"); importer.Importxls_Bracing(sheet, ctx); break;
                                case "walls": completed++; ProgressHub.Send($"Importing Walls: {(completed * 100) / totalSections}%"); importer.Importxls_Walls(sheet, ctx); break;
                                case "surfaces": completed++; ProgressHub.Send($"Importing Surfaces: {(completed * 100) / totalSections}%"); importer.Importxls_Surfaces(sheet, ctx); break;
                                case "atticrooms": completed++; ProgressHub.Send($"Importing Attic Rooms: {(completed * 100) / totalSections}%"); importer.Importxls_AtticRooms(sheet, ctx); break;
                                case "roofingdata": completed++; ProgressHub.Send($"Importing Roofing Data: {(completed * 100) / totalSections}%"); importer.Importxls_RoofingData(sheet, ctx); break;
                                case "sundryitems": completed++; ProgressHub.Send($"Importing Sundry Items: {(completed * 100) / totalSections}%"); importer.Importxls_SundryItems(sheet, ctx); break;
                                case "fasteners": completed++; ProgressHub.Send($"Importing Fasteners: {(completed * 100) / totalSections}%"); importer.Importxls_Fasteners(sheet, ctx); break;
                                case "doorsandwindows": completed++; ProgressHub.Send($"Importing Doors and Windows: {(completed * 100) / totalSections}%"); importer.Importxls_DoorsandWindows(sheet, ctx); break;
                                case "decking": completed++; ProgressHub.Send($"Importing Decking: {(completed * 100) / totalSections}%"); importer.Importxls_Decking(sheet, ctx); break;
                                case "sheeting": completed++; ProgressHub.Send($"Importing Sheeting: {(completed * 100) / totalSections}%"); importer.Importxls_Sheeting(sheet, ctx); break;
                                case "co2material": completed++; ProgressHub.Send($"Importing CO2 Material: {(completed * 100) / totalSections}%"); importer.Importxls_CO2material(sheet, ctx); break;
                                default:
                                    System.Diagnostics.Debug.WriteLine($"No importer implemented for sheet: {sheetName}");
                                    break;
                            }
                        }

                        // (Optional) success banner
                        TempData["Message"] = $"Imported project {projectCode} v{hubVersion.VersionNo}.";
                        ProgressHub.Send("IMPORT_COMPLETE");
                    }
                }
                catch (Exception ex)
                {
                    using (var db = new AppDbContext())
                    {
                        db.ImportFailures.Add(new ImportFailure
                        {
                            ProjectId = "NA",
                            Section = "UploadExcel",
                            ErrorMessage = ex.Message,
                            TimeStamp = DateTime.Now
                        });
                        db.SaveChanges();
                    }
                    TempData["Error"] = $"Error importing Excel: {ex.Message}";
                }
            }

            return RedirectToAction("ImportExcel");
        }

        private static string ComputeSha256(Stream s)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            // If you’ll reuse the stream after hashing, reset position before/after
            if (s.CanSeek) s.Position = 0;

            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hash = sha.ComputeHash(s);
                if (s.CanSeek) s.Position = 0;
                return BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        //
        // Back-compat overload (matches your current call site)
        //

        private (Project Project, ProjectVersion Version, string DuplicateInfo)
            EnsureProjectAndNextVersion(
                AppDbContext db,
                string projectCode,
                string client,
                string site,
                string fileHash)
        {
            var ctx = System.Web.HttpContext.Current;

            // ImportedBy: prefer web user, else machine user
            var importedBy = (ctx?.User?.Identity?.Name);
            if (string.IsNullOrWhiteSpace(importedBy))
                importedBy = Environment.UserName;

            // Try get the uploaded filename from the current request
            string originalName = null;
            try
            {
                if (ctx?.Request?.Files != null && ctx.Request.Files.Count > 0)
                {
                    var f = ctx.Request.Files[0];
                    if (f != null && !string.IsNullOrWhiteSpace(f.FileName))
                        originalName = Path.GetFileName(f.FileName);
                }
            }
            catch
            {
                // swallow — filename is nice-to-have
            }

            // Forward to the full helper (7-arg)
            return EnsureProjectAndNextVersion(
                db,
                projectCode,
                client,
                site,
                fileHash,
                originalName,   // now populated from the request when available
                importedBy
            );
        }

        //
        // Full helper (preferred) – call this if you can pass file name & user
        //
        private (Project Project, ProjectVersion Version, string DuplicateInfo)
                EnsureProjectAndNextVersion(
                    AppDbContext db,
                    string projectCode,
                    string client,
                    string site,
                    string fileHash,
                    string originalFileName,
                    string importedBy)
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                throw new ArgumentException("projectCode required", nameof(projectCode));

            projectCode = projectCode.Trim();
            client = (client ?? string.Empty).Trim();
            site = (site ?? string.Empty).Trim();

            // 1) Project: create or backfill
            var prj = db.Projects.FirstOrDefault(p => p.ProjectCode == projectCode);
            if (prj == null)
            {
                prj = new Project
                {
                    ProjectCode = projectCode,
                    ClientName = string.IsNullOrWhiteSpace(client) ? null : client,
                    SiteName = string.IsNullOrWhiteSpace(site) ? null : site,
                    CreatedAt = DateTime.UtcNow
                };
                db.Projects.Add(prj);
                db.SaveChanges();
            }
            else
            {
                bool changed = false;
                if (!string.IsNullOrWhiteSpace(client) && string.IsNullOrWhiteSpace(prj.ClientName))
                {
                    prj.ClientName = client; changed = true;
                }
                if (!string.IsNullOrWhiteSpace(site) && string.IsNullOrWhiteSpace(prj.SiteName))
                {
                    prj.SiteName = site; changed = true;
                }
                if (changed) db.SaveChanges();
            }

            // 2) Duplicate file check (per project)
            var dup = db.ProjectImportBatches
                        .FirstOrDefault(b => b.ProjectId == prj.Id && b.FileHash == fileHash);
            if (dup != null)
            {
                var vno = db.ProjectVersions
                            .Where(v => v.Id == dup.ProjectVersionId)
                            .Select(v => v.VersionNo)
                            .FirstOrDefault();
                return (prj, null, $"Identical file was already imported for project {projectCode} (v{vno}).");
            }

            // 3) Next version
            var nextNo = db.ProjectVersions
                           .Where(v => v.ProjectId == prj.Id)
                           .Select(v => (int?)v.VersionNo)
                           .Max()
                           .GetValueOrDefault(0) + 1;

            var ver = new ProjectVersion
            {
                ProjectId = prj.Id,
                VersionNo = nextNo,
                CreatedAt = DateTime.UtcNow
            };
            db.ProjectVersions.Add(ver);
            db.SaveChanges();

            // 4) Batch audit
            var batch = new ProjectImportBatch
            {
                ProjectId = prj.Id,
                ProjectVersionId = ver.Id,
                OriginalFileName = string.IsNullOrWhiteSpace(originalFileName) ? null : originalFileName,
                FileHash = fileHash,
                ImportedBy = string.IsNullOrWhiteSpace(importedBy) ? null : importedBy,
                ImportedAt = DateTime.UtcNow
            };
            db.ProjectImportBatches.Add(batch);
            db.SaveChanges();

            return (prj, ver, null);
        }


        #region Import Excel Sheet TABS Stubs


        #endregion

        private void SetProperty(GeneralProjectData project, string propertyName, string value)
        {
            var property = typeof(GeneralProjectData).GetProperty(propertyName);
            if (property != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    property.SetValue(project, value);
                }
                else if (property.PropertyType == typeof(double?) &&
                         double.TryParse(value.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double d))
                {
                    property.SetValue(project, d);
                }
                else if (property.PropertyType == typeof(int?) &&
                         int.TryParse(value, out int i))
                {
                    property.SetValue(project, i);
                }
                // Add other types as needed (DateTime?, bool?, etc.)
            }
        }

        private double? ParseDouble(string input)
        {
            if (double.TryParse(input.Replace(",", "."), System.Globalization.NumberStyles.Any,
                                System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return null;
        }

        public ActionResult ListExcel()
        {
            var list = db.GeneralProjectDatas
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new ExcelProjectListItemViewModel
                {
                    Data = g,
                    QuotesGenerated = db.Quotes
                        .Count(q => q.ProjectId == g.Id) // adjust if needed
                })
                .ToList();

            return View(list);
        }

        public ActionResult DetailsExcel(int id)
        {
            var general = db.GeneralProjectDatas.Find(id);
            if (general == null) return HttpNotFound();

            var vm = new DetailsExcelViewModel
            {
                GeneralData = general,
                Sections = new Dictionary<string, List<object>>(),
                ProjectImages = db.ProjectImages
                    .Where(p => p.GeneralProjectDataId == id)
                    .ToList()
            };

            // Define deferred queries using lambdas
            var sectionDefinitions = new List<(string SectionName, Func<List<object>> SectionQuery)>
            {
                ("Framing Zones", () => db.xls_FramingZones.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Frames", () => db.xls_Frames.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Manufacture Frames", () => db.xls_Manufactureframes.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Timber", () => db.xls_Timber.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Cladding", () => db.xls_Cladding.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Boards", () => db.xls_Boards.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Connector Plates", () => db.xls_ConnectorPlates.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Posi Struts", () => db.xls_PosiStruts.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Metalwork", () => db.xls_Metalwork.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Bracing", () => db.xls_Bracing.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Walls", () => db.xls_Walls.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Surfaces", () => db.xls_Surfaces.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Attic Rooms", () => db.xls_AtticRooms.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Roofing Data", () => db.xls_RoofingData.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Sundry Items", () => db.xls_SundryItems.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Fasteners", () => db.xls_Fasteners.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Doors and Windows", () => db.xls_DoorsandWindows.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Decking", () => db.xls_Decking.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("Sheeting", () => db.xls_Sheeting.Where(x => x.GeneralProjectDataId == id).ToList<object>()),
                ("CO2 Material", () => db.xls_CO2material.Where(x => x.GeneralProjectDataId == id).ToList<object>())
            };

            // Execute each query and add to Sections
            foreach (var (sectionName, sectionQuery) in sectionDefinitions)
            {
                var sectionData = sectionQuery();
                vm.Sections.Add(sectionName, sectionData);
            }

            return View("DetailsExcel", vm);
        }

        //JSON Import
        public ActionResult ListJson()
        {
            var list = db.GeneralQuoteDatas
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new ProjectListItemViewModel
                {
                    Data = g,
                    QuotesGenerated = db.Quotes
                        .Count(q => q.ProjectId == g.Id) // count all quotes for this project
                })
                .ToList();

            return View(list);
        }

        public ActionResult ImportJson()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase jsonFile)
        {
            if (jsonFile == null || jsonFile.ContentLength == 0 || !jsonFile.FileName.EndsWith(".json"))
            {
                TempData["Error"] = "Please upload a valid JSON file.";
                return RedirectToAction("Index");
            }

            try
            {
                string jsonContent;
                using (var reader = new StreamReader(jsonFile.InputStream))
                {
                    jsonContent = reader.ReadToEnd();
                }

                // Pre-clean malformed number formats
                jsonContent = PreCleanJson(jsonContent);

                var root = JsonConvert.DeserializeObject<RootObject>(jsonContent);

                if (root?.ProjectData == null || root.ProjectData.Count == 0)
                {
                    TempData["Error"] = "Invalid JSON structure.";
                    return RedirectToAction("Index");
                }

                // 1. Import GeneralData
                var generalBlock = root.ProjectData.FirstOrDefault(p => p.GeneralData?.Any() == true);
                if (generalBlock == null)
                {
                    TempData["Error"] = "General quote data missing.";
                    return RedirectToAction("Index");
                }

                var generalData = generalBlock.GeneralData.First();
                generalData.CreatedAt = DateTime.Now;

                // Versioning logic
                var existingVersions = db.GeneralQuoteDatas.Where(g =>
                    g.ProjectName == generalData.ProjectName &&
                    g.SiteAddress == generalData.SiteAddress
                ).ToList();

                generalData.Version = existingVersions.Any()
                    ? existingVersions.Max(g => g.Version) + 1
                    : 1;

                db.GeneralQuoteDatas.Add(generalData);
                ProgressHub.Send($"General Quote Data Imported, Starting Sections");
                db.SaveChanges();
                int projectId = generalData.Id ?? 0;

                // 2. Flatten all sections and import once
                int totalSections = 21;
                int currentSection = 0;

                void ImportWithProgress<T>(List<T> data, string name) where T : class, IGeneralProjectDataBound
                {
                    currentSection++;
                    int percent = (currentSection * 100 / totalSections);
                    int count = data?.Count ?? 0;
                    ProgressHub.Send($"Importing {name} ({count} rows) - {percent}%");
                    if (count > 0) ImportSection(data, projectId, name);
                }

                ImportWithProgress(root.ProjectData.SelectMany(b => b.FramingZonesTable ?? new List<FramingZones>()).ToList(), "FRAMING_ZONES_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.FramesTable ?? new List<Frames>()).ToList(), "FRAMES_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.ManufacturingFramesTable ?? new List<ManufacturingFrames>()).ToList(), "MANUFACTURING_FRAMES_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.TimberTable ?? new List<Timber>()).ToList(), "TIMBER_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.ConnectorsTable ?? new List<Connectors>()).ToList(), "CONNECTORS_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.MetalworkTable ?? new List<Metalwork>()).ToList(), "METALWORK_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.BracingTable ?? new List<Bracing>()).ToList(), "BRACING_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.WallsTable ?? new List<Walls>()).ToList(), "WALLS_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.SurfacesTable ?? new List<Surfaces>()).ToList(), "SURFACES_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.RoofingDataTable ?? new List<RoofingData>()).ToList(), "ROOFING_DATA_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.FastenersTable ?? new List<Fasteners>()).ToList(), "FASTENERS_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.SheetingTable ?? new List<Sheeting>()).ToList(), "SHEETING_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.Co2MaterialTable ?? new List<Co2Material>()).ToList(), "CO2_MATERIAL_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.AtticRoomsTable ?? new List<AtticRoom>()).ToList(), "ATTIC_ROOMS_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.BoardsTable ?? new List<Board>()).ToList(), "BOARDS_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.CladdingTable ?? new List<Cladding>()).ToList(), "CLADDING_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.DeckingTable ?? new List<Decking>()).ToList(), "DECKING_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.OpeningFrameLabelTable ?? new List<OpeningFrameLabel>()).ToList(), "OPENING_FRAME_LABEL");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.PosiStrutTable ?? new List<PosiStrut>()).ToList(), "POSI_STRUT_TABLE");
                ImportWithProgress(root.ProjectData.SelectMany(b => b.SundryItemsTable ?? new List<SundryItem>()).ToList(), "SUNDRY_ITEMS_TABLE");


                // Email Genetaion
                string templatePath = Server.MapPath("~/Templates/Emails/QuoteImportedTemplate.html");
                string htmlBody = System.IO.File.ReadAllText(templatePath)
                    .Replace("{{ProjectName}}", generalData.ProjectName)
                    .Replace("{{Designer}}", generalData.Designer)
                    .Replace("{{SiteAddress}}", generalData.SiteAddress)
                    .Replace("{{Version}}", generalData.Version.ToString())
                    .Replace("{{ImportedBy}}", Session["FullName"]?.ToString() ?? "Unknown")
                    .Replace("{{DateImported}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                    .Replace("{{QuoteLink}}", $"https://cloud-mroofing.co.za/Account/Login?returnUrl=/Import/DetailsJson/{projectId}");

                //Email Service
                EmailService.SendQuoteImportNotification(
                    subject: $"New Quote Imported: {generalData.ProjectName}",
                    htmlBody: htmlBody
                );

                TempData["Message"] = "JSON imported successfully.";
                TempData["ImportedId"] = projectId;
                TempData["ImportedProjectName"] = generalData.ProjectName;
                TempData["ImportedVersion"] = generalData.Version;
                TempData["ShowImportSuccessModal"] = true;
                ProgressHub.Send("IMPORT_COMPLETE");

                return RedirectToAction("ImportJson");
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors.Select(ve =>
                        $"Entity: {e.Entry.Entity.GetType().Name}, Property: {ve.PropertyName}, Error: {ve.ErrorMessage}"
                    ))
                    .ToList();

                var fullErrorMessage = string.Join("; ", errorMessages);

                db.ImportFailures.Add(new ImportFailure
                {
                    ProjectId = "NA", // or "Unknown" if projectId not available
                    Section = "Upload",
                    ErrorMessage = "Validation error: " + fullErrorMessage,
                    TimeStamp = DateTime.Now
                });
                db.SaveChanges();

                TempData["Error"] = "Validation failed: " + fullErrorMessage;
                return RedirectToAction("Index");
            }

        }

        private static string PreCleanJson(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
                return rawJson;

            // 1. Fix quoted numbers with semicolon/comma: "PITCH": "26;49.58" or "26,49.58" → 26.49
            rawJson = Regex.Replace(
                rawJson,
                @"""(?<key>\w+)""\s*:\s*""(?<whole>\d+)[;,](?<frac>\d+)(.*?)""",
                m => $"\"{m.Groups["key"].Value}\": {m.Groups["whole"].Value}.{m.Groups["frac"].Value}",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // 2. Convert known numeric fields from quoted integers → numbers (like "1" → 1)
            rawJson = Regex.Replace(
                rawJson,
                @"""(?<key>PART_NUMBER|MEMBER_LAYERS)""\s*:\s*""(?<val>\d+)""",
                @"""${key}"": ${val}",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // 3. Convert quoted floats like "5.2" → 5.2
            rawJson = Regex.Replace(
                rawJson,
                @"""(?<key>\w+)""\s*:\s*""(?<val>\d+\.\d+)""",
                @"""${key}"": ${val}",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // 4. Normalize string-based "null", "N/A", or empty → null
            rawJson = Regex.Replace(
                rawJson,
                @"""(\w+)""\s*:\s*""\s*(N/A|null)?\s*""",
                "\"$1\": null",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            return rawJson;
        }

        private void ImportSection<T>(IEnumerable<T> data, int projectId, string sectionName) where T : class, IGeneralProjectDataBound
        {
            if (data == null) return;

            try
            {
                foreach (var item in data)
                {
                    item.GeneralProjectDataId = projectId;
                    db.Set<T>().Add(item);
                }
                Debug.WriteLine("Tracking Timber count: " + db.ChangeTracker.Entries<Timber>().Count());

                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(e => e.ValidationErrors.Select(ve =>
                        $"Entity: {e.Entry.Entity.GetType().Name}, Property: {ve.PropertyName}, Error: {ve.ErrorMessage}"
                    ))
                    .ToList();

                var fullErrorMessage = string.Join("; ", errorMessages);

                db.ImportFailures.Add(new ImportFailure
                {
                    ProjectId = projectId.ToString(),
                    Section = sectionName,
                    ErrorMessage = fullErrorMessage,
                    TimeStamp = DateTime.Now
                });

                db.SaveChanges();

                TempData["Error"] = $"Validation failed while importing {sectionName}.";
            }
        }

        public ActionResult DetailsJson(int id)
        {
            var general = db.GeneralQuoteDatas.Find(id);
            if (general == null) return HttpNotFound();

            var vm = new DetailsJsonViewModel
            {
                GeneralData = general,
                Sections = new Dictionary<string, List<object>>
                {
                    { "Framing Zones", db.FramingZones.Where(z => z.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Frames", db.Frames.Where(f => f.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Manufacturing Frames", db.ManufacturingFrames.Where(m => m.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Timber", db.Timbers.Where(t => t.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Connectors", db.Connectors.Where(c => c.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Metalwork", db.Metalworks.Where(m => m.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Bracing", db.Bracings.Where(b => b.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Walls", db.Walls.Where(w => w.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Surfaces", db.Surfaces.Where(s => s.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Roofing", db.RoofingData.Where(r => r.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Fasteners", db.Fasteners.Where(f => f.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Sheeting", db.Sheetings.Where(s => s.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "CO2 Materials", db.Co2Materials.Where(c => c.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Attic Rooms", db.AtticRooms.Where(a => a.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Boards", db.Boards.Where(b => b.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Claddings", db.Claddings.Where(c => c.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Deckings", db.Deckings.Where(d => d.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Opening Frame Labels", db.OpeningFrameLabels.Where(o => o.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "PosiStruts", db.PosiStruts.Where(p => p.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                    { "Sundry Items", db.SundryItems.Where(s => s.GeneralProjectDataId == id).ToList().Cast<object>().ToList() },
                },
                ProjectImages = db.ProjectImages
                      .Where(p => p.GeneralProjectDataId == id)
                      .ToList()
            };

            return View(vm);
        }

        #region New 3-file Quote Import (Quote.xlsx, QuoteLines.xlsx, QuoteLineAggregate.xlsx)

        private static decimal NormalizeVat(decimal? v)
        {
            if (!v.HasValue) return 0m;
            var n = v.Value;
            return n >= 1m ? n / 100m : n; // accept 15 or 0.15
        }


        [HttpGet]
        public ActionResult ImportQuoteFiles()
        {
            return View(); // make a simple view with ProjectId, QuoteId, 3 file inputs, replaceExisting checkbox
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ImportQuoteFiles(
            int projectId,
            int quoteId,
            HttpPostedFileBase quoteFile,      // Quote.xlsx  -> ProjectQuoteLineHeader + patch onto ProjectQuotes
            HttpPostedFileBase linesFile,      // QuoteLines.xlsx -> ProjectQuoteLineItem
            HttpPostedFileBase aggregateFile,  // QuoteLineAggregate.xlsx -> ProjectQuoteLineAggregate
            bool replaceExisting = false)
        {
            if ((quoteFile == null || quoteFile.ContentLength == 0) &&
                (linesFile == null || linesFile.ContentLength == 0) &&
                (aggregateFile == null || aggregateFile.ContentLength == 0))
            {
                TempData["Error"] = "Upload at least one file (Quote.xlsx, QuoteLines.xlsx, or QuoteLineAggregate.xlsx).";
                return RedirectToAction("Edit", "ProjectQuotes", new { id = quoteId });
            }

            try
            {
                ExcelPackage.License.SetNonCommercialOrganization("MR DEV");

                var importedBy = User?.Identity?.Name ?? Environment.UserName;

                // Replace-existing: wipe only the tables for which we received a file
                if (replaceExisting)
                {
                    if (quoteFile != null && quoteFile.ContentLength > 0)
                    {
                        var wipeH = db.ProjectQuoteLineHeaders.Where(x => x.ProjectId == projectId && x.QuoteId == quoteId);
                        db.ProjectQuoteLineHeaders.RemoveRange(wipeH);
                        db.SaveChanges();
                    }
                    if (linesFile != null && linesFile.ContentLength > 0)
                    {
                        var wipeI = db.ProjectQuoteLineItems.Where(x => x.ProjectId == projectId && x.QuoteId == quoteId);
                        db.ProjectQuoteLineItems.RemoveRange(wipeI);
                        db.SaveChanges();
                    }
                    if (aggregateFile != null && aggregateFile.ContentLength > 0)
                    {
                        var wipeA = db.ProjectQuoteLineAggregates.Where(x => x.ProjectId == projectId && x.QuoteId == quoteId);
                        db.ProjectQuoteLineAggregates.RemoveRange(wipeA);
                        db.SaveChanges();
                    }
                }

                int headerRows = 0, lineRows = 0, aggRows = 0;

                // ----- Header (Quote.xlsx) -----
                if (quoteFile != null && quoteFile.ContentLength > 0)
                {
                    QuoteHeaderPatch patch;
                    headerRows = ImportQuoteHeaderWorkbook(quoteFile, projectId, quoteId, importedBy, out patch);

                    // push header values into ProjectQuotes so the edit form shows them immediately
                    if (headerRows > 0 && patch != null)
                        ApplyHeaderToProjectQuote(quoteId, patch);
                }

                // ----- Lines (QuoteLines.xlsx) -----
                if (linesFile != null && linesFile.ContentLength > 0)
                    lineRows = ImportQuoteLinesWorkbook(linesFile, projectId, quoteId, importedBy);

                // ----- Aggregate (QuoteLineAggregate.xlsx) -----
                if (aggregateFile != null && aggregateFile.ContentLength > 0)
                    aggRows = ImportQuoteAggregateWorkbook(aggregateFile, projectId, quoteId, importedBy);

                TempData["ToastMessage"] =
                    $"Import complete:<br>" +
                    $"Header rows: {headerRows}<br>" +
                    $"Line rows: {lineRows}<br>" +
                    $"Aggregate rows: {aggRows}";

                // Reload the Edit page so the form textboxes show the updated values
                return RedirectToAction("Edit", "ProjectQuotes", new { id = quoteId });
            }
            catch (Exception ex)
            {
                db.ImportFailures.Add(new ImportFailure
                {
                    ProjectId = projectId.ToString(),
                    Section = "ImportQuoteFiles",
                    ErrorMessage = ex.Message,
                    TimeStamp = DateTime.Now
                });
                db.SaveChanges();

                TempData["ToastError"] = $"Import failed: {ex.Message}";
                return RedirectToAction("Edit", "ProjectQuotes", new { id = quoteId });
            }
        }


        // =========================
        // Workbooks → Table imports
        // =========================
        // Converts value -> target property type (handles string <-> decimal? both ways)
        private static object ConvertToType(object value, Type targetType)
        {
            if (value == null) return null;

            var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                // Target is string
                if (t == typeof(string))
                {
                    if (value is string s) return s;
                    if (value is IFormattable fmt) return fmt.ToString("0.##", CultureInfo.InvariantCulture);
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
                }

                // Target is decimal (or decimal?)
                if (t == typeof(decimal))
                {
                    if (value is decimal d) return d;

                    if (value is string str)
                    {
                        str = str.Trim();
                        if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv) ||
                            decimal.TryParse(str, NumberStyles.Any, CultureInfo.CurrentCulture, out dv))
                        {
                            return dv;
                        }
                        return null; // can't parse -> skip
                    }

                    // other numeric types
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }

                // If incoming is empty string for a non-string target, treat as null
                if (value is string s2 && string.IsNullOrWhiteSpace(s2)) return null;

                // Fallback: change type
                return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null; // conversion failed -> skip assignment
            }
        }

        private static void SetProp(object obj, string propName, object value)
        {
            if (value == null) return;
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
            if (prop == null || !prop.CanWrite) return;

            var converted = ConvertToType(value, prop.PropertyType);
            if (converted != null) prop.SetValue(obj, converted, null);
        }

        private void ApplyHeaderToProjectQuote(int quoteId, QuoteHeaderPatch p)
        {
            if (p == null) return;

            var pq = db.ProjectQuotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (pq == null) return;

            // We don't care whether these props are string or decimal? on ProjectQuotes.
            // SetProp converts safely based on the actual property type.
            SetProp(pq, "RoofPitch", p.RoofPitch);      // string OR decimal?
            SetProp(pq, "RoofOverhang", p.RoofOverhang);   // decimal?
            SetProp(pq, "GableOverhang", p.GableOverhang);  // decimal?
            SetProp(pq, "RoofCovering", p.RoofCovering);   // string
            SetProp(pq, "MaxBattensCc", p.MaxBattensCc);   // decimal?
            SetProp(pq, "MaxTrussesCc", p.MaxTrussesCc);   // decimal?
            SetProp(pq, "RoofArea", p.RoofArea);       // decimal?
            SetProp(pq, "FloorArea", p.FloorArea);      // decimal?

            db.SaveChanges();
        }

        private int ImportQuoteHeaderWorkbook(
                    HttpPostedFileBase file, int projectId, int quoteId, string importedBy,
                    out QuoteHeaderPatch patch)
        {
            patch = new QuoteHeaderPatch();

            using (var pkg = new ExcelPackage(file.InputStream))
            {
                var ws = pkg.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new InvalidOperationException("Quote.xlsx: No worksheet found.");

                // The header labels we care about (aliases already handled by your fuzzy helpers)
                var required = new[]
                {
            "Roof Pitch","Roof Overhang","Roof Gable Overhang","Roof Covering",
            "Max Batten Centres","Max Truss Centres","Floor Area","Roof Area"
        };

                // Find header row fuzzily (scan a little deeper just in case)
                int headerRow = FindHeaderRowFuzzy(ws, required, 150);
                if (headerRow < 0)
                    headerRow = FindHeaderRowFuzzyAtLeast(ws, required, minHits: 3, scanRows: 200);

                var col = BuildColMapFuzzy(ws, headerRow);
                var dim = ws.Dimension;
                if (dim == null) return 0;

                int inserted = 0;

                // Read FIRST non-empty data row after header
                for (int r = headerRow + 1; r <= dim.End.Row; r++)
                {
                    if (RowIsEmptyFuzzy(ws, r, required, col)) continue;

                    // Persist an audit row to ProjectQuoteLineHeader (keep columns you actually have)
                    var entity = new MillenniumWebFixed.Models.Projects.ProjectQuoteLineHeader
                    {
                        ProjectId = projectId,
                        QuoteId = quoteId,
                        ImportedBy = importedBy,

                        // Map to your entity’s properties (rename if yours differ)
                        RoofPitch = GetDec(ws, r, col, "Roof Pitch"),
                        RoofOverhang = GetDec(ws, r, col, "Roof Overhang"),
                        RoofGableOverhang = GetDec(ws, r, col, "Roof Gable Overhang"),
                        RoofCovering = GetText(ws, r, col, "Roof Covering"),
                        MaxBattenCentres = GetDec(ws, r, col, "Max Batten Centres"),
                        MaxTrussCentres = GetDec(ws, r, col, "Max Truss Centres"),
                        FloorArea = GetDec(ws, r, col, "Floor Area"),
                        RoofArea = GetDec(ws, r, col, "Roof Area"),
                    };

                    db.ProjectQuoteLineHeaders.Add(entity);
                    inserted++;

                    // Fill the patch to push onto ProjectQuotes
                    // entity.RoofPitch is decimal? → string with suffix for the form
                    patch.RoofPitch = entity.RoofPitch.HasValue
                        ? entity.RoofPitch.Value.ToString("0.##", CultureInfo.InvariantCulture) + " Deg"
                        : null;
                    patch.GableOverhang = entity.RoofGableOverhang;
                    patch.RoofCovering = entity.RoofCovering;
                    patch.MaxBattensCc = entity.MaxBattenCentres;
                    patch.MaxTrussesCc = entity.MaxTrussCentres;
                    patch.FloorArea = entity.FloorArea;
                    patch.RoofArea = entity.RoofArea;

                    break; // only first non-empty data row
                }

                db.SaveChanges();
                return inserted;
            }
        }


        private int ImportQuoteLinesWorkbook(HttpPostedFileBase file, int projectId, int quoteId, string importedBy)
        {
            // Must-have columns to import correctly
            var essential = new[] {
                        "Quantity","Select Product","Price Per Unit","Cost Per Unit","VAT","Product ID","Product Name"
                    };
            // Nice-to-have (import if present)
            var optional = new[] { "Quote", "Price Overridden", "Line", "Notes" };

            using (var pkg = new ExcelPackage(file.InputStream))
            {
                var ws = pkg.Workbook.Worksheets.FirstOrDefault();
                if (ws == null) throw new InvalidOperationException("QuoteLines.xlsx: No worksheet found.");

                // find header row using fuzzy matching
                int headerRow = FindHeaderRowFuzzy(ws, essential, 200); // scan deeper just in case
                if (headerRow < 0)
                    headerRow = FindHeaderRowFuzzyAtLeast(ws, essential, minHits: Math.Max(3, essential.Length - 2), scanRows: 200);

                // build canonical column map
                var col = BuildColMapFuzzy(ws, headerRow);

                // ensure all essential columns were mapped
                var missing = essential.Where(k => !col.ContainsKey(k)).ToList();
                if (missing.Any())
                    throw new InvalidOperationException("QuoteLines.xlsx: Missing required columns: " + string.Join(", ", missing)
                                                       + ". Found: " + DumpHeadersRow(ws, headerRow));

                int inserted = 0;
                var dim = ws.Dimension;
                var keysForEmptyCheck = essential.Concat(optional).ToArray();

                for (int r = headerRow + 1; r <= dim.End.Row; r++)
                {
                    // skip empty rows (don’t break; some files have footer gaps)
                    if (RowIsEmptyFuzzy(ws, r, keysForEmptyCheck, col)) continue;

                    // Add temporarily where you call GetDec, for the first few rows:
                    var cell = ws.Cells[r, col["Price Per Unit"]];
                    var valType = cell.Value?.GetType().Name ?? "null";

                    var entity = new Models.Projects.ProjectQuoteLineItem
                    {
                        ProjectId = projectId,
                        QuoteId = quoteId,
                        ImportedBy = importedBy,

                        QuoteExcel = GetText(ws, r, col, "Quote"),
                        Quantity = GetDec(ws, r, col, "Quantity"),
                        SelectProduct = GetText(ws, r, col, "Select Product"),
                        PriceOverridden = GetBool(ws, r, col, "Price Overridden"),
                        PricePerUnit = GetDec(ws, r, col, "Price Per Unit"),
                        CostPerUnit = GetDec(ws, r, col, "Cost Per Unit"),
                        Vat = GetDec(ws, r, col, "VAT"),
                        ProductIdExcel = GetText(ws, r, col, "Product ID"),
                        ProductName = GetText(ws, r, col, "Product Name"),
                        LineNotes = GetText(ws, r, col, "Line Notes") ?? GetText(ws, r, col, "Notes")
                    };

                    db.ProjectQuoteLineItems.Add(entity);
                    inserted++;
                }

                db.SaveChanges();
                return inserted;
            }
        }

        private int ImportQuoteAggregateWorkbook(HttpPostedFileBase file, int projectId, int quoteId, string importedBy)
        {
            // Your file headers (no "Price"): make Price OPTIONAL
            var essential = new[]
            {
        "Quantity","Select Product","Price Per Unit","Cost Per Unit","VAT","Product ID","Product Name"
    };
            var optional = new[] { "Quote", "Price", "Price Overridden", "Overridden", "Line Notes" };

            using (var pkg = new ExcelPackage(file.InputStream))
            {
                var ws = pkg.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new InvalidOperationException("QuoteLineAggregate.xlsx: No worksheet found.");

                // find header row (essentials only)
                int headerRow = FindHeaderRowFuzzy(ws, essential, 200);
                if (headerRow < 0)
                    headerRow = FindHeaderRowFuzzyAtLeast(ws, essential, minHits: Math.Max(3, essential.Length - 2), scanRows: 200);
                if (headerRow < 0)
                    throw new InvalidOperationException("QuoteLineAggregate.xlsx: Could not find the header row.");

                var col = BuildColMapFuzzy(ws, headerRow);

                // ensure essentials are present
                var missing = essential.Where(k => !col.ContainsKey(k)).ToList();
                if (missing.Any())
                    throw new InvalidOperationException(
                        "QuoteLineAggregate.xlsx: Missing required columns: " + string.Join(", ", missing) +
                        ". Found: " + DumpHeadersRow(ws, headerRow));

                int inserted = 0;
                var dim = ws.Dimension;
                var keysForEmptyCheck = essential.Concat(optional).ToArray();

                for (int r = headerRow + 1; r <= dim.End.Row; r++)
                {
                    if (RowIsEmptyFuzzy(ws, r, keysForEmptyCheck, col)) continue;

                    // compute Price if not supplied
                    var qty = GetDec(ws, r, col, "Quantity") ?? 0m;
                    var ppu = GetDec(ws, r, col, "Price Per Unit") ?? 0m;
                    var price = GetDec(ws, r, col, "Price") ?? (qty * ppu);

                    var entity = new Models.Projects.ProjectQuoteLineAggregate
                    {
                        ProjectId = projectId,
                        QuoteId = quoteId,
                        ImportedBy = importedBy,

                        QuoteExcel = GetText(ws, r, col, "Quote"),
                        Quantity = qty,
                        SelectProduct = GetText(ws, r, col, "Select Product"),
                        Price = price,
                        // accept either header name for the flag
                        Overridden = GetBool(ws, r, col, "Price Overridden") ?? GetBool(ws, r, col, "Overridden"),
                        PricePerUnit = ppu,
                        CostPerUnit = GetDec(ws, r, col, "Cost Per Unit"),
                        Vat = GetDec(ws, r, col, "VAT"),
                        ProductIdExcel = GetText(ws, r, col, "Product ID"),
                        ProductName = GetText(ws, r, col, "Product Name"),
                        LineNotes = GetText(ws, r, col, "Line Notes") ?? GetText(ws, r, col, "Notes")
                    };

                    db.ProjectQuoteLineAggregates.Add(entity);
                    inserted++;
                }

                db.SaveChanges();
                return inserted;
            }
        }

        // ==================
        // FUZZY header helpers
        // ==================
        private static string Norm(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            // lower, keep letters+digits only
            var chars = s.ToLowerInvariant().Where(ch => char.IsLetterOrDigit(ch));
            return new string(chars.ToArray());
        }

        // ==================
        // EPPlus helpers
        // ==================

        private static int FindHeaderRowExact(ExcelWorksheet ws, string[] required, int scanRows = 10)
        {
            var dim = ws.Dimension;
            if (dim == null) return -1;
            int lastRow = Math.Min(dim.End.Row, scanRows);

            for (int r = 1; r <= lastRow; r++)
            {
                var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int c = 1; c <= dim.End.Column; c++)
                {
                    var text = (ws.Cells[r, c].Text ?? "").Trim();
                    if (!string.IsNullOrEmpty(text)) found.Add(text);
                }
                if (required.All(h => found.Contains(h))) return r;
            }
            return -1;
        }


        // Canonical names -> accepted aliases (normalized via Norm)
        private static readonly Dictionary<string, string[]> HeaderAliases = new Dictionary<string, string[]>
        {
            // Lines / Aggregate common
            ["Quote"] = new[] { "quote", "quoteno", "quotenumber" },
            ["Quantity"] = new[] { "quantity", "qty", "qnty" },
            ["Select Product"] = new[] { "selectproduct", "product", "item", "itemname" },
            ["Price Overridden"] = new[] { "priceoverridden", "overridden", "override", "isoverridden", "priceoverride" },
            ["Price Per Unit"] = new[] { "priceperunit", "ppu", "unitprice", "priceunit", "priceeach" },
            ["Cost Per Unit"] = new[] { "costperunit", "cpu", "unitcost", "costunit", "costeach" },
            ["VAT"] = new[] { "vat", "vatpercent", "vat", "tax", "taxrate" },
            ["Product ID"] = new[] { "productid", "productcode", "itemcode", "sku" },
            ["Product Name"] = new[] { "productname", "itemdescription", "description", "name" },
            ["Line"] = new[] { "line", "lineno", "linenumber" },
            ["Notes"] = new[] { "notes", "note", "remarks", "comment", "comments" },

            // Header (Quote.xlsx)
            ["Quote ID"] = new[] { "quoteid", "quoteref" },
            ["Roof Pitch"] = new[] { "roofpitch", "pitch" },
            ["Roof Overhang"] = new[] { "roofoverhang", "overhang" },
            ["Roof Gable Overhang"] = new[] { "roofgableoverhang", "gableoverhang" },
            ["Roof Covering"] = new[] { "roofcovering", "covering" },
            ["Max Batten Centres"] = new[] { "maxbattencentres", "maxbattenscc", "battenscc" },
            ["Max Truss Centres"] = new[] { "maxtrusscentres", "maxtrussescc", "trussescc" },
            ["Floor Area"] = new[] { "floorarea" },
            ["Roof Area"] = new[] { "roofarea" },
            ["E-Finks (Work Units)"] = new[] { "efinksworkunits", "efinks", "workunits" },
            ["E-Finks Cost"] = new[] { "efinkscost" },
            ["Transport Cost"] = new[] { "transportcost", "deliverycost", "freight" },

            // Aggregate (QuoteLineAggregate.xlsx) specific
            ["Price"] = new[] { "price", "linetotal", "lineprice", "amount" },
            ["Overridden"] = new[] { "overridden", "override", "priceoverridden" },
            ["Line Notes"] = new[] { "linenotes", "notes", "remark", "remarks", "comment", "comments" },
        };


        // Dump the literal headers in the detected header row (useful in error messages)
        private static string DumpHeadersRow(ExcelWorksheet ws, int headerRow)
        {
            var dim = ws.Dimension;
            var vals = new List<string>();
            for (int c = 1; c <= dim.End.Column; c++)
            {
                var t = (ws.Cells[headerRow, c].Text ?? "").Trim();
                if (!string.IsNullOrEmpty(t)) vals.Add(t);
            }
            return string.Join(", ", vals);
        }

        private static int FindHeaderRowFuzzy(ExcelWorksheet ws, IEnumerable<string> canonicalRequired, int scanRows = 50)
        {
            var dim = ws.Dimension;
            if (dim == null) return -1;
            int lastRow = Math.Min(dim.End.Row, scanRows);

            var reqNormSets = canonicalRequired
                .Select(c => HeaderAliases.TryGetValue(c, out var arr) ? arr.Select(Norm).ToHashSet()
                                                                       : new HashSet<string> { Norm(c) })
                .ToList();

            for (int r = 1; r <= lastRow; r++)
            {
                var found = new HashSet<string>();
                for (int c = 1; c <= dim.End.Column; c++)
                {
                    var text = (ws.Cells[r, c].Text ?? "").Trim();
                    if (!string.IsNullOrEmpty(text)) found.Add(Norm(text));
                }
                if (reqNormSets.All(set => set.Any(a => found.Contains(a) || found.Any(f => f.Contains(a)))))
                    return r;
            }
            return -1;
        }

        private static int FindHeaderRowFuzzyAtLeast(ExcelWorksheet ws, IEnumerable<string> canonicalRequired, int minHits, int scanRows = 50)
        {
            var dim = ws.Dimension;
            if (dim == null) return -1;

            var reqNormSets = canonicalRequired
                .Select(c => HeaderAliases.TryGetValue(c, out var arr) ? arr.Select(Norm).ToHashSet()
                                                                       : new HashSet<string> { Norm(c) })
                .ToList();

            int lastRow = Math.Min(dim.End.Row, scanRows);
            for (int r = 1; r <= lastRow; r++)
            {
                var found = new HashSet<string>();
                for (int c = 1; c <= dim.End.Column; c++)
                {
                    var text = (ws.Cells[r, c].Text ?? "").Trim();
                    if (!string.IsNullOrEmpty(text)) found.Add(Norm(text));
                }

                // count how many essential headers are satisfied on this row
                int hits = reqNormSets.Count(set => set.Any(a => found.Contains(a)));
                if (hits >= minHits) return r;
            }
            return -1;
        }

        private static Dictionary<string, int> BuildColMapFuzzy(ExcelWorksheet ws, int headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var dim = ws.Dimension;
            if (dim == null) return map;

            for (int c = 1; c <= dim.End.Column; c++)
            {
                var raw = (ws.Cells[headerRow, c].Text ?? "").Trim();
                if (string.IsNullOrEmpty(raw)) continue;

                // 1) add the raw header (exact)
                if (!map.ContainsKey(raw)) map[raw] = c;

                // 2) add the canonical key if the raw header matches an alias
                var n = Norm(raw);
                foreach (var kv in HeaderAliases)
                {
                    if (kv.Value.Any(a => a == n))
                    {
                        if (!map.ContainsKey(kv.Key)) map[kv.Key] = c;
                    }
                }
            }
            return map;
        }


        private static bool RowIsEmptyFuzzy(ExcelWorksheet ws, int r, IEnumerable<string> keys, Dictionary<string, int> col)
        {
            foreach (var k in keys)
                if (col.TryGetValue(k, out var c) && !string.IsNullOrWhiteSpace((ws.Cells[r, c].Text ?? "").Trim()))
                    return false;
            return true;
        }

        private static bool RowIsEmpty(ExcelWorksheet ws, int r, string[] headers, Dictionary<string, int> col)
        {
            foreach (var h in headers)
            {
                if (col.TryGetValue(h, out var c))
                {
                    var t = (ws.Cells[r, c].Text ?? "").Trim();
                    if (!string.IsNullOrEmpty(t)) return false;
                }
            }
            return true;
        }

        private static string GetText(ExcelWorksheet ws, int row, IDictionary<string, int> col, string key)
        {
            if (!TryResolveCol(col, key, out var c)) return null;
            var t = (ws.Cells[row, c].Text ?? "").Trim();
            return string.IsNullOrWhiteSpace(t) ? null : t;
        }

        private static decimal? GetDec(OfficeOpenXml.ExcelWorksheet ws, int row,
                                       IDictionary<string, int> col, string key)
        {
            try
            {
                if (!col.TryGetValue(key, out var c) || c <= 0) return null;

                var cell = ws.Cells[row, c];
                if (cell == null) return null;

                var val = cell.Value;

                // 1) If Excel stored a numeric, use it directly (no GetValue<T>())
                switch (val)
                {
                    case null:
                        return null;

                    case double d:
                        return Convert.ToDecimal(d, CultureInfo.InvariantCulture);

                    case decimal dm:
                        return dm;

                    case float f:
                        return Convert.ToDecimal(f, CultureInfo.InvariantCulture);

                    case int i:
                        return i;

                    case long l:
                        return l;

                    case short s:
                        return s;

                    case byte b:
                        return b;

                        // fall through for strings and other types
                }

                // 2) Fallback: parse text safely
                var raw = (cell.Text ?? val.ToString() ?? "").Trim();
                if (raw.Length == 0) return null;

                // Remove NBSP and regular spaces (used as grouping)
                raw = raw.Replace("\u00A0", " ").Replace(" ", "");

                // If both separators present, treat the last one as decimal separator
                if (raw.Contains(",") && raw.Contains("."))
                {
                    var lastDot = raw.LastIndexOf('.');
                    var lastComma = raw.LastIndexOf(',');

                    char dec = lastDot > lastComma ? '.' : ',';
                    char grp = dec == '.' ? ',' : '.';

                    raw = raw.Replace(grp.ToString(), "");  // remove grouping separator
                    if (dec == ',') raw = raw.Replace(',', '.'); // unify to '.'
                }
                else if (raw.Contains(",") && !raw.Contains("."))
                {
                    // Decimal comma only
                    raw = raw.Replace(",", ".");
                }
                else
                {
                    // Only dot or plain digits — strip any stray commas
                    raw = raw.Replace(",", "");
                }

                if (decimal.TryParse(raw, NumberStyles.Number | NumberStyles.AllowLeadingSign,
                                     CultureInfo.InvariantCulture, out var result))
                    return result;

                // Final sanitization attempt
                var cleaned = new string(raw.Where(ch => char.IsDigit(ch) || ch == '.' || ch == '-').ToArray());
                return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowLeadingSign,
                                        CultureInfo.InvariantCulture, out result)
                       ? result
                       : (decimal?)null;
            }
            catch (Exception ex)
            {
                // Lightweight diagnostic for .NET Framework
                Debug.WriteLine($"[GetDec ERR] Row {row} Key '{key}' -> {ex.Message}");
                return null;
            }
        }

        private static int? GetInt(ExcelWorksheet ws, int row, IDictionary<string, int> col, string key)
        {
            var d = GetDec(ws, row, col, key);
            return d.HasValue ? (int?)Convert.ToInt32(d.Value) : null;
        }

        private static bool? GetBool(ExcelWorksheet ws, int row, IDictionary<string, int> col, string key)
        {
            if (!TryResolveCol(col, key, out var c)) return null;

            var cell = ws.Cells[row, c];
            if (cell == null) return null;
            var v = cell.Value;
            if (v == null) return null;

            if (v is bool b) return b;
            if (v is double dd) return Math.Abs(dd) > double.Epsilon;
            if (v is int ii) return ii != 0;
            if (v is decimal mm) return mm != 0m;

            var s = (cell.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(s)) return null;

            switch (s.ToLowerInvariant())
            {
                case "true":
                case "yes":
                case "y":
                case "t":
                    return true;
                case "false":
                case "no":
                case "n":
                case "f":
                    return false;
            }
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                return num != 0m;

            return null;
        }

        private static bool TryResolveCol(IDictionary<string, int> col, string key, out int c)
        {
            // exact hit first
            if (col.TryGetValue(key, out c)) return c > 0;

            // try aliases for the key
            if (HeaderAliases.TryGetValue(key, out var alts))
            {
                foreach (var alt in alts)
                {
                    // try by raw alias text
                    if (col.TryGetValue(alt, out c) && c > 0) return true;

                    // try normalized compare against existing keys
                    var altNorm = alt; // already normalized
                    foreach (var k in col.Keys)
                    {
                        if (Norm(k) == altNorm) { c = col[k]; return c > 0; }
                    }
                }
            }

            // final: normalized compare of key against existing keys
            var keyNorm = Norm(key);
            foreach (var k in col.Keys)
            {
                if (Norm(k) == keyNorm) { c = col[k]; return c > 0; }
            }

            c = 0;
            return false;
        }

        #endregion


    }
}
