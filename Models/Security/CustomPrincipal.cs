using System.Security.Principal;

namespace MillenniumWebFixed.Security
{
    public class CustomPrincipal : GenericPrincipal
    {
        public CustomPrincipal(IIdentity identity) : base(identity, null) { }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string UserLevel { get; set; }
        public string Email { get; set; }
        public bool IsSalesRep { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
    }
}
