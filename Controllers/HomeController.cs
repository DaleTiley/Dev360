
using MillenniumWebFixed.Models;
using System;
using System.Linq;
using System.Web.Mvc;
namespace MillenniumWebFixed.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Index()
        {
            var oneHourAgo = DateTime.Now.AddMinutes(-60);

            // Count JSON For Dashboard Badges
            int newJsonQuotes = db.GeneralQuoteDatas
                .Count(q => q.CreatedAt >= oneHourAgo);

            ViewBag.NewJsonQuotes = newJsonQuotes;

            // Count Excel For Dashboard Badges
            int newExcelQuotes = db.GeneralProjectDatas
                .Count(q => q.CreatedAt >= oneHourAgo);

            ViewBag.NewExcelQuotes = newExcelQuotes;

            //Dahsboard Insights
            ViewBag.StockCount = db.Products.Count();
            ViewBag.ProjectCount = db.Projects.Count();
            ViewBag.UserCount = db.AppUsers.Count();
            ViewBag.TenderCount = "0";
            ViewBag.QuoteCount = db.Quotes.Count();
            ViewBag.AssemblyCount = db.ProductAssemblies.Count();
            ViewBag.ExcelProjectCount = db.Projects.Count();
            ViewBag.ExcelQuoteCount = "0";
            ViewBag.OrderCount = "0";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About Millennium Roofing.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact page.";
            return View();
        }
    }
}
