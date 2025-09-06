using MillenniumWebFixed.Models;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    public class Enquiry
    {
        public int EnquiryId { get; set; }
        public string EnquiryNumber { get; set; }
        public int CustomerId { get; set; }
        public int SalesRepUserId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Customer Customer { get; set; }

        [ForeignKey("SalesRepUserId")]
        public virtual AppUser SalesRep { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual AppUser CreatedByUser { get; set; }
    }
}
