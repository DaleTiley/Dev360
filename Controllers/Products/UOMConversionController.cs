using MillenniumWebFixed.Models;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class UOMConversionController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: UOMConversion
        public ActionResult Index()
        {
            var conversions = db.UOMConversions
                .Include("ItemType") // ✅ EF6-style Include
                .ToList();

            ViewBag.ItemTypes = db.ItemTypes
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                }).ToList();

            ViewBag.UOMs = db.UOMs
                .Select(u => new SelectListItem
                {
                    Value = u.UOMCode,
                    Text = u.UOMCode + " - " + u.Description
                }).ToList();

            return View("~/Views/StockAdmin/UOMConversions.cshtml", conversions);
        }

        // GET: UOMConversion/Get/5
        public JsonResult Get(int id)
        {
            var conv = db.UOMConversions
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.ItemTypeId,
                    c.FromUOM,
                    c.ToUOM,
                    c.Formula,
                    c.RequiresProperties,
                    c.Notes
                })
                .FirstOrDefault();

            if (conv == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            return Json(conv, JsonRequestBehavior.AllowGet);
        }

        // POST: UOMConversion/Save
        [HttpPost]
        public JsonResult Save(UOMConversion model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Invalid data" });
            }

            if (model.Id == 0)
            {
                db.UOMConversions.Add(model);
            }
            else
            {
                var existing = db.UOMConversions.Find(model.Id);
                if (existing == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Not found" });
                }

                existing.ItemTypeId = model.ItemTypeId;
                existing.FromUOM = model.FromUOM;
                existing.ToUOM = model.ToUOM;
                existing.Formula = model.Formula;
                existing.RequiresProperties = model.RequiresProperties;
                existing.Notes = model.Notes;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        // POST: UOMConversion/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var conv = db.UOMConversions.Find(id);
            if (conv == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Item not found" });
            }

            db.UOMConversions.Remove(conv);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
