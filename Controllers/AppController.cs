using MillenniumWebFixed.Security;
using System.Web.Mvc;

public abstract class AppController : Controller
{
    // Hide Controller.User (IPrincipal) with your CustomPrincipal
    protected new CustomPrincipal User => base.User as CustomPrincipal;

    protected override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        ViewBag.Me = User; // optional
        base.OnActionExecuting(filterContext);
    }
}
