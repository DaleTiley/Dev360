using MillenniumWebFixed.Models;
using System.Linq;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ProductAssemblyController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpPost]
        public ActionResult Create(int productId, string name)
        {
            var assembly = new ProductAssembly
            {
                ProductId = productId,
                Name = name
            };

            db.ProductAssemblies.Add(assembly);
            db.SaveChanges();

            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var assembly = db.ProductAssemblies
                .Include("Components")
                .FirstOrDefault(a => a.Id == id);

            if (assembly == null)
                return HttpNotFound();

            // Manually delete all child components
            foreach (var component in assembly.Components.ToList())
            {
                db.ProductAssemblyComponents.Remove(component);
            }

            db.ProductAssemblies.Remove(assembly);
            db.SaveChanges();

            return Json(new { success = true });
        }


    }

}