using MillenniumWebFixed.Models;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ItemTypeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        // GET: ItemType
        public ActionResult Index()
        {
            var types = db.ItemTypes.ToList();
            return View("~/Views/StockAdmin/ItemTypes.cshtml", types);
        }

        // GET: ItemType/Get/5 (AJAX)
        public JsonResult Get(int id)
        {
            var item = db.ItemTypes
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.BaseUOM,
                    t.SupportsM2,
                    t.SupportsM3,
                    t.Notes
                })
                .FirstOrDefault();

            if (item == null)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            return Json(item, JsonRequestBehavior.AllowGet);
        }

        // POST: ItemType/Save (AJAX)
        [HttpPost]
        public JsonResult Save(ItemType model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        var errors = string.Join(", ", state.Errors.Select(e => e.ErrorMessage));
                        System.Diagnostics.Debug.WriteLine($"{key}: {errors}");
                    }
                }
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Invalid data" });
            }

            if (model.Id == 0)
            {
                db.ItemTypes.Add(model);
            }
            else
            {
                var existing = db.ItemTypes.Find(model.Id);
                if (existing == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { success = false, message = "Not found" });
                }

                existing.Name = model.Name;
                existing.BaseUOM = model.BaseUOM;
                existing.SupportsM2 = model.SupportsM2;
                existing.SupportsM3 = model.SupportsM3;
                existing.Notes = model.Notes;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        // POST: ItemType/Delete/5 (AJAX)
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var item = db.ItemTypes.Find(id);
            if (item == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Item not found" });
            }

            db.ItemTypes.Remove(item);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
