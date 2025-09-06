using MillenniumWebFixed.Helpers;
using MillenniumWebFixed.Models;
using MillenniumWebFixed.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            return View();
        }


        // --- CUSTOMERS ---
        public ActionResult Customers()
        {
            // your existing customers query:
            var customers = db.Customers.AsNoTracking()
                .OrderBy(c => c.CustomerName)
                .ToList();

            // count projects per customer by matching name
            var projCountsByName = db.Projects.AsNoTracking()
                .Where(p => p.ClientName != null && p.ClientName != "")
                .GroupBy(p => p.ClientName)
                .Select(g => new { ClientName = g.Key, Count = g.Count() })
                .ToList();

            // map counts to customer IDs (using name equality)
            var counts = customers.ToDictionary(
                c => c.Id,
                c => projCountsByName.FirstOrDefault(x =>
                        x.ClientName.Equals(c.CustomerName, StringComparison.OrdinalIgnoreCase)
                    )?.Count ?? 0
            );

            ViewBag.ProjectCounts = counts;   // IDictionary<int,int>
            return View(customers);           // or your VM
        }

        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult CustomerProjects(int id)
        {
            // Get the customer name (or return empty set for smooth UX)
            var name = db.Customers.AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => c.CustomerName)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(name))
                return PartialView("~/Views/Admin/Partials/_CustomerProjectsRows.cshtml",
                    Enumerable.Empty<CustomerProjectRowVm>());

            // Pull only what we need from DB
            var raw = db.Projects.AsNoTracking()
                .Where(p => p.ClientName == name)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.ProjectCode,
                    p.SiteName,
                    p.SiteAddress1,
                    p.SiteAddress2,
                    p.SiteCity,
                    p.SiteProvince,
                    p.SitePostcode,
                    p.Status,
                    p.CreatedAt,
                    p.ContactName,
                    p.ContactEmail,
                    p.ContactPhone
                })
                .ToList(); // materialize once

            // Compose view rows (address + status fallback)
            var rows = raw.Select(p => new CustomerProjectRowVm
            {
                Id = p.Id,
                ProjectCode = p.ProjectCode,
                SiteName = p.SiteName,
                Address = string.Join(", ",
                    new[] { p.SiteAddress1, p.SiteAddress2, p.SiteCity, p.SiteProvince, p.SitePostcode }
                        .Where(s => !string.IsNullOrWhiteSpace(s))),
                Status = string.IsNullOrWhiteSpace(p.Status) ? "Active" : p.Status.Trim(),
                CreatedAt = p.CreatedAt,
                ContactName = p.ContactName,
                ContactEmail = p.ContactEmail,
                ContactPhone = p.ContactPhone
            }).ToList();

            return PartialView("~/Views/Admin/Partials/_CustomerProjectsRows.cshtml", rows);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCustomer(Customer model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    model.CreatedAt = DateTime.Now;
                    db.Customers.Add(model);
                    TempData["Message"] = "Customer added successfully.";
                }
                else
                {
                    var existing = db.Customers.Find(model.Id);
                    if (existing == null) return HttpNotFound();

                    existing.CustomerName = model.CustomerName;
                    existing.CustomerCode = model.CustomerCode;
                    existing.VATNumber = model.VATNumber;
                    existing.AddressLine1 = model.AddressLine1;
                    existing.AddressLine2 = model.AddressLine2;
                    existing.City = model.City;
                    existing.StateProvince = model.StateProvince;
                    existing.PostalCode = model.PostalCode;
                    existing.Country = model.Country;
                    existing.ContactPerson = model.ContactPerson;
                    existing.Email = model.Email;
                    existing.Phone = model.Phone;
                    existing.IsActive = model.IsActive;
                    existing.Notes = model.Notes;
                    existing.UpdatedAt = DateTime.Now;

                    TempData["Message"] = "Customer updated successfully.";
                }

                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Please complete all required fields.";
            }

            return RedirectToAction("Customers");
        }

        public ActionResult GetCustomer(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return Json(customer, JsonRequestBehavior.AllowGet);
        }

        // Contacts
        public ActionResult Contacts()
        {
            var contacts = db.Contacts.Include("Customer").OrderBy(c => c.Name).ToList();
            ViewBag.Customers = db.Customers.OrderBy(c => c.CustomerName).ToList(); // for dropdown
            return View(contacts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveContact(Contact model)
        {
            if (ModelState.IsValid)
            {
                if (model.ContactId == 0)
                {
                    model.CreatedAt = DateTime.Now;
                    model.CreatedByUserId = 1; // TODO: get from logged-in user
                    db.Contacts.Add(model);
                    TempData["Message"] = "Contact added successfully.";
                }
                else
                {
                    var existing = db.Contacts.Find(model.ContactId);
                    if (existing == null) return HttpNotFound();

                    existing.Name = model.Name;
                    existing.Designation = model.Designation;
                    existing.PhoneNumber = model.PhoneNumber;
                    existing.Email = model.Email;
                    existing.CustomerId = model.CustomerId;
                    // Do not update CreatedAt/CreatedBy
                    TempData["Message"] = "Contact updated successfully.";
                }

                db.SaveChanges();
            }
            else
            {
                TempData["Error"] = "Please complete all required fields.";
            }

            return RedirectToAction("Contacts");
        }

        public ActionResult GetContact(int id)
        {
            var contact = db.Contacts.Include("Customer").FirstOrDefault(c => c.ContactId == id);
            if (contact == null) return HttpNotFound();
            return Json(contact, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Users()
        {
            var users = db.AppUsers.ToList();
            return View(users);
        }

        public ActionResult AddUser()
        {
            ModelState.Clear();
            return View(new AppUser());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUser(AppUser user)
        {
            if (ModelState.IsValid)
            {
                // Hash the incoming password
                if (!string.IsNullOrWhiteSpace(user.PasswordHash)) // Assume temporary plain password in PasswordHash
                {
                    string hashed = PasswordHelper.HashPassword(user.PasswordHash);
                    user.PasswordHash = hashed;
                }

                user.CreatedDate = DateTime.Now;
                db.AppUsers.Add(user);
                db.SaveChanges();

                TempData["Message"] = "User added successfully.";
                return RedirectToAction("Users");
            }

            TempData["Error"] = "Invalid user data.";
            return RedirectToAction("AddUser");
        }


        public ActionResult EditUser(int id)
        {
            var user = db.AppUsers.Find(id);
            if (user == null) return HttpNotFound();
            return View(user); // Or PartialView if using modal
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser(AppUser model)
        {
            // Remove PasswordHash from model validation if optional
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
            {
                return PartialView("_EditUserPartial", model);
            }

            var user = db.AppUsers.Find(model.Id);
            if (user == null)
                return HttpNotFound();

            // Update fields
            user.Username = model.Username;
            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserLevel = model.UserLevel;
            user.IsActive = model.IsActive;
            user.IsDesigner = model.IsDesigner;
            user.IsSalesRep = model.IsSalesRep;
            user.IsEstimator = model.IsEstimator;
            user.IsQuoteApprover = model.IsQuoteApprover;
            user.Designation = model.Designation;
            user.Department = model.Department;

            // Only hash and update password if entered
            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                user.PasswordHash = PasswordHelper.HashPassword(model.PasswordHash);
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        public ActionResult EditUserModal(int id)
        {
            var user = db.AppUsers.Find(id);
            if (user == null) return HttpNotFound();

            return PartialView("_EditUserPartial", user);
        }

        [HttpPost]
        public ActionResult CleanData()
        {
            try
            {
                db.Database.ExecuteSqlCommand("EXEC sp_Delete_AllProjectData");
                TempData["Message"] = "All project data deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting data: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        //Quote --> Product Mapping
        // Product Mappings
        public ActionResult ProductMappings()
        {
            var mappings = db.ProductMappings.OrderByDescending(m => m.CreatedAt).ToList();
            return View(mappings);
        }

        public ActionResult AddProductMapping()
        {
            return View(new ProductMapping());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProductMapping(ProductMapping model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                db.ProductMappings.Add(model);
                db.SaveChanges();
                TempData["Message"] = "Mapping added successfully.";
                return RedirectToAction("ProductMappings");
            }
            return View(model);
        }

        public ActionResult EditProductMapping(int id)
        {
            var mapping = db.ProductMappings.Find(id);
            if (mapping == null) return HttpNotFound();
            return View(mapping);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProductMapping(ProductMapping model)
        {
            if (ModelState.IsValid)
            {
                var original = db.ProductMappings.Find(model.Id);
                if (original == null) return HttpNotFound();

                // Update only the editable fields
                original.JsonItemName = model.JsonItemName;
                original.ProductDesc = model.ProductDesc;
                original.Notes = model.Notes;
                // DO NOT overwrite CreatedAt

                db.SaveChanges();
                TempData["Message"] = "Mapping updated successfully.";
                return RedirectToAction("ProductMappings");
            }

            return View(model);
        }


        public ActionResult DeleteProductMapping(int id)
        {
            var mapping = db.ProductMappings.Find(id);
            if (mapping == null) return HttpNotFound();
            db.ProductMappings.Remove(mapping);
            db.SaveChanges();
            TempData["Message"] = "Mapping deleted.";
            return RedirectToAction("ProductMappings");
        }


    }
}
