using MillenniumWebFixed.Models;
using System.Linq;
using System.Web.Mvc;

public class ProductController : Controller
{
    private readonly AppDbContext db = new AppDbContext();

    [HttpGet]
    public JsonResult ListAll()
    {
        var products = db.Products
            .OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name })
            .ToList();

        return Json(products, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult GetAll()
    {
        var products = db.Products
            .Select(p => new { p.Id, p.Name, p.ProductCode })
            .OrderBy(p => p.Name)
            .ToList();

        return Json(products, JsonRequestBehavior.AllowGet);
    }

    [HttpGet]
    public JsonResult Search(string term)
    {
        var query = db.Products.AsQueryable();

        if (!string.IsNullOrEmpty(term))
            query = query.Where(p => p.Name.Contains(term));

        var products = query
            .OrderBy(p => p.Name)
            .Take(50)
            .Select(p => new
            {
                id = p.Id,
                text = p.Name
            })
            .ToList();

        return Json(products, JsonRequestBehavior.AllowGet);
    }


    [HttpGet]
    public JsonResult GetProperty(int id)
    {
        var property = db.ProductProperties
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                p.PropertyName,
                p.Value,
                p.Units,
                p.ProductId,
                ProductName = p.Product.Name // Add this line
            })
            .FirstOrDefault();

        if (property == null)
            return Json(new { error = "Not found" }, JsonRequestBehavior.AllowGet);

        return Json(property, JsonRequestBehavior.AllowGet);
    }



}
