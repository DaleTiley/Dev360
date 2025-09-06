using MillenniumWebFixed.Models;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ProductPropertyController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpPost]
        public ActionResult Create(int productId, string propertyName, string value, string units)
        {
            var prop = new ProductProperty
            {
                ProductId = productId,
                PropertyName = propertyName,
                Value = value,
                Units = units
            };

            db.ProductProperties.Add(prop);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Update(int id, int productId, string value, string units)
        {
            var prop = db.ProductProperties.Find(id);
            if (prop == null)
                return HttpNotFound();

            prop.Value = value;
            prop.Units = units;
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var prop = db.ProductProperties.Find(id);
            if (prop == null)
                return HttpNotFound();

            db.ProductProperties.Remove(prop);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
