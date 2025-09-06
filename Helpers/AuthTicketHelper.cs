using MillenniumWebFixed.Models;
using MillenniumWebFixed.Security;
using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

public static class AuthTicketHelper
{
    private const char Delim = '|';

    // Order matters – keep it in sync
    public static string PackUserData(AppUser u) => string.Join(Delim.ToString(), new[]
    {
        u.Id.ToString(),                     // 0
        u.FullName ?? "",                    // 1
        u.UserLevel ?? "",                   // 2
        u.Email ?? "",                       // 3
        (u.IsSalesRep ? "1" : "0"),          // 4
        u.Designation ?? "",                 // 5
        u.Department ?? ""                   // 6
    });

    public static CustomPrincipal ToPrincipal(FormsAuthenticationTicket ticket)
    {
        var p = new CustomPrincipal(new GenericIdentity(ticket.Name));
        var parts = (ticket.UserData ?? "").Split(Delim);

        if (parts.Length > 0 && int.TryParse(parts[0], out var id)) p.Id = id;
        if (parts.Length > 1) p.FullName = parts[1];
        if (parts.Length > 2) p.UserLevel = parts[2];
        if (parts.Length > 3) p.Email = parts[3];
        if (parts.Length > 4) p.IsSalesRep = parts[4] == "1";
        if (parts.Length > 5) p.Designation = parts[5];
        if (parts.Length > 6) p.Department = parts[6];

        return p;
    }

    public static void IssueAuthCookie(string username, string userData, bool persistent = false, int minutes = 30)
    {
        var ticket = new FormsAuthenticationTicket(
            1, username, DateTime.Now, DateTime.Now.AddMinutes(minutes), persistent, userData);

        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
        {
            HttpOnly = true,
            Secure = FormsAuthentication.RequireSSL,
            Path = FormsAuthentication.FormsCookiePath
        };
        HttpContext.Current.Response.Cookies.Add(cookie);
    }
}
