using System;

namespace MillenniumWebFixed.ViewModels
{
    public class CustomerProjectRowVm
    {
        public int Id { get; set; }
        public string ProjectCode { get; set; }
        public string SiteName { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }
}
