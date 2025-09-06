using MillenniumWebFixed.Models;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class UOMController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: UOM
        public ActionResult Index()
        {
            var uoms = db.UOMs.ToList();
            return View("~/Views/StockAdmin/UOMs.cshtml", uoms);
        }

        // GET: UOM/Get/{code}
        public JsonResult Get(string id)
        {
            var uom = db.UOMs
                .Where(u => u.UOMCode == id)
                .Select(u => new
                {
                    u.UOMCode,
                    u.Description,
                    u.BaseUnit
                })
                .FirstOrDefault();

            if (uom == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            return Json(uom, JsonRequestBehavior.AllowGet);
        }

        // POST: UOM/Save
        [HttpPost]
        public JsonResult Save(UOM model)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Invalid data" });
            }

            var existing = db.UOMs.Find(model.UOMCode);
            if (existing == null)
            {
                db.UOMs.Add(model);
            }
            else
            {
                existing.Description = model.Description;
                existing.BaseUnit = model.BaseUnit;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        // POST: UOM/Delete/{code}
        [HttpPost]
        public JsonResult Delete(string id)
        {
            var uom = db.UOMs.Find(id);
            if (uom == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Item not found" });
            }

            db.UOMs.Remove(uom);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
