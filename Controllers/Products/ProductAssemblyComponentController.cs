using MillenniumWebFixed.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace MillenniumWebFixed.Controllers
{
    public class ProductAssemblyComponentController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        [HttpPost]
        public ActionResult Create(int assemblyId, int componentProductId, decimal qtyPerUOM, string unit, decimal costPerUnit, string totalFormula, string notes)
        {
            try
            {
                var comp = new ProductAssemblyComponent
                {
                    AssemblyId = assemblyId,
                    ComponentProductId = componentProductId,
                    QtyPerUOM = qtyPerUOM,
                    Unit = unit,
                    CostPerUnit = costPerUnit,
                    TotalFormula = totalFormula,
                    Notes = notes
                };

                db.ProductAssemblyComponents.Add(comp);
                db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message); // You’ll see the real cause now
            }
        }

        [HttpGet]
        public JsonResult GetComponent(int id)
        {
            var comp = db.ProductAssemblyComponents
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    id = c.Id,
                    assemblyId = c.AssemblyId,
                    componentProductId = c.ComponentProductId,
                    qtyPerUOM = c.QtyPerUOM,
                    unit = c.Unit,
                    costPerUnit = c.CostPerUnit,
                    totalFormula = c.TotalFormula,
                    notes = c.Notes
                })
                .FirstOrDefault();

            return Json(comp, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(ProductAssemblyComponent model)
        {
            var comp = db.ProductAssemblyComponents.Find(model.Id);
            if (comp == null) return HttpNotFound();

            comp.QtyPerUOM = model.QtyPerUOM;
            comp.Unit = model.Unit;
            comp.CostPerUnit = model.CostPerUnit;
            comp.TotalFormula = model.TotalFormula;
            comp.Notes = model.Notes;

            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var comp = db.ProductAssemblyComponents.Find(id);
            if (comp == null) return HttpNotFound();

            db.ProductAssemblyComponents.Remove(comp);
            db.SaveChanges();

            return Json(new { success = true });
        }

    }
}