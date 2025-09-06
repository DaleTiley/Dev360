using MillenniumWebFixed.Helpers;
using MillenniumWebFixed.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace MillenniumWebFixed.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext db = new AppDbContext();

        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl; // keep it for the form hidden field
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            email = email?.Trim();
            password = password ?? string.Empty;

            var user = db.AppUsers.FirstOrDefault(u => u.Username == email);
            if (user == null) { TempData["Error"] = "User not found."; return View(); }

            var hash = user.PasswordHash?.Trim();
            if (string.IsNullOrWhiteSpace(hash) || !PasswordHelper.VerifyPassword(password, hash))
            { TempData["Error"] = "Incorrect password."; return View(); }

            // Success
            AuthTicketHelper.IssueAuthCookie(user.Username, AuthTicketHelper.PackUserData(user));
            user.LastLoginDate = DateTime.Now;
            db.SaveChanges();

            // ---- normalize returnUrl (absolute → relative) & prevent open redirect
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                // If absolute and same host, convert to relative
                if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var abs) && Request?.Url != null &&
                    abs.Host.Equals(Request.Url.Host, StringComparison.OrdinalIgnoreCase))
                {
                    returnUrl = abs.PathAndQuery;
                }

                // Handle any accidental double-encoding (optional but handy)
                returnUrl = System.Web.HttpUtility.UrlDecode(returnUrl);

                if (Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }
    }
}
