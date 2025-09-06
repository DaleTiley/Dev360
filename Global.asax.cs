using MillenniumWebFixed.Models;
using System;
using System.Data.Entity;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace MillenniumWebFixed
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            var culture = new System.Globalization.CultureInfo("en-ZA");
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }

        protected void Application_Start()
        {
            ModelBinders.Binders[typeof(decimal)] = new FlexibleDecimalBinder();
            ModelBinders.Binders[typeof(decimal?)] = new FlexibleDecimalBinder();
            Database.SetInitializer<AppDbContext>(null); //Don’t touch or manage the schema anymore
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null) return;

            var ticket = FormsAuthentication.Decrypt(cookie.Value);
            if (ticket == null) return;

            HttpContext.Current.User = AuthTicketHelper.ToPrincipal(ticket);
        }

        private sealed class FlexibleDecimalBinder : IModelBinder
        {
            public object BindModel(ControllerContext cc, ModelBindingContext ctx)
            {
                var vp = ctx.ValueProvider.GetValue(ctx.ModelName);
                if (vp == null) return null;
                var s = (vp.AttemptedValue ?? "").Trim();
                if (s.Length == 0) return null;
                s = s.Replace(" ", "").Replace("\u00A0", "").Replace("\u202F", "").Replace(',', '.');
                return decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d)
                    ? (object)d
                    : Add(ctx, "Enter a valid number (e.g. 123.45)");
            }
            private object Add(ModelBindingContext ctx, string msg)
            { ctx.ModelState.AddModelError(ctx.ModelName, msg); return null; }
        }
    }
}
