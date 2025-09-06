using MillenniumWebFixed.Security;
using System.Security.Principal;

namespace MillenniumWebFixed.Helpers
{
    public static class PrincipalExtensions
    {
        public static int GetUserId(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.Id : 0;
        }

        public static string GetFullName(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.FullName : string.Empty;
        }

        public static string GetUserLevel(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.UserLevel : string.Empty;
        }

        public static string GetEmail(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.Email : string.Empty;
        }

        public static bool IsSalesRep(this IPrincipal user)
        {
            return user is CustomPrincipal me && me.IsSalesRep;
        }

        public static string GetDesignation(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.Designation : string.Empty;
        }

        public static string GetDepartment(this IPrincipal user)
        {
            return user is CustomPrincipal me ? me.Department : string.Empty;
        }
    }
}
