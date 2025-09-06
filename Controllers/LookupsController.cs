using MillenniumWebFixed.ViewModels;
using MillenniumWebFixed.Models; // <-- your AppDbContext namespace
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class LookupsController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: /Lookups/CustomerRows?q=&page=1&pageSize=25
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> CustomerRows(string q = "", int page = 1, int pageSize = 25)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 25;

            var query = db.Customers.AsNoTracking().Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(c =>
                       c.CustomerName.Contains(q)
                    || c.CustomerCode.Contains(q)
                    || c.City.Contains(q)
                    || c.StateProvince.Contains(q));
            }

            var rows = await query
                .OrderBy(c => c.CustomerName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                })
                .ToListAsync();

            return PartialView("~/Views/Lookups/_CustomerRows.cshtml", rows);
        }

        // GET: /Lookups/ContactRows?clientId=123&q=&page=1&pageSize=25
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public async Task<ActionResult> ContactRows(int? clientId, string q = "", int page = 1, int pageSize = 25)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 25;

            var query = db.Contacts.AsNoTracking().AsQueryable();

            if (clientId.HasValue) query = query.Where(x => x.CustomerId == clientId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(x =>
                       x.Name.Contains(q)
                    || x.Email.Contains(q)
                    || x.PhoneNumber.Contains(q)
                    || x.Designation.Contains(q));
            }

            var rows = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ContactDropdownItem
                {
                    ContactId = x.ContactId,
                    Name = x.Name,
                    Designation = x.Designation,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    CustomerId = x.CustomerId
                })
                .ToListAsync();

            return PartialView("~/Views/Lookups/_ContactRows.cshtml", rows);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
