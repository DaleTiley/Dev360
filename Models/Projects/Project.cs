using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class Project
    {
        public int Id { get; set; }

        // Core
        [Required, StringLength(100)]
        public string ProjectCode { get; set; }    // e.g., S04001-5 or your own code

        [StringLength(200)]
        public string ClientName { get; set; }

        [StringLength(200)]
        public string SiteName { get; set; }       // optional display name of site

        // Primary Contact (client)
        [StringLength(100)]
        public string ContactName { get; set; }

        [StringLength(150), EmailAddress]
        public string ContactEmail { get; set; }

        [StringLength(50)]
        public string ContactPhone { get; set; }
        public string Designer { get; set; }

        // Address (legacy kept, plus StreetAddress added below)
        [StringLength(200)] public string SiteAddress1 { get; set; }
        [StringLength(200)] public string SiteAddress2 { get; set; }
        [StringLength(100)] public string SiteCity { get; set; }
        [StringLength(20)] public string SitePostcode { get; set; }
        [StringLength(100)] public string SiteProvince { get; set; }

        public string Notes { get; set; }

        [StringLength(30)]
        public string Status { get; set; }  // Active/On Hold/Closed

        public DateTime CreatedAt { get; set; }

        // -------- NEW FIELDS (per client sketch) --------

        // Site/Legal
        [StringLength(50)] public string ERPNumber { get; set; }
        [StringLength(100)] public string Township { get; set; }
        [StringLength(50)] public string PortionNumber { get; set; }
        [StringLength(50)] public string StandNumber { get; set; }

        // Explicit street line (keeps Address1/2 for legacy)
        [StringLength(200)] public string StreetAddress { get; set; }

        // Links
        [StringLength(500), Url] public string GoogleMapUrl { get; set; }
        [StringLength(500), Url] public string SharePointUrl { get; set; }

        // Commercial
        [StringLength(100)] public string SiteRentals { get; set; }

        // People
        [StringLength(120)] public string SalesPerson { get; set; }
        [StringLength(120)] public string SiteContactName { get; set; }
        [StringLength(50)] public string SiteContactPhone { get; set; }

        // CRM-lite
        [StringLength(50)] public string CrmStage { get; set; }         // Lead/Quoted/Won/Lost
        [StringLength(120)] public string CrmNextAction { get; set; }    // e.g., Call client
        public DateTime? CrmFollowUpDate { get; set; }

        public string CreatedBy { get; set; }            // nvarchar(100)
        public int? CreatedByUserId { get; set; }        // optional
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
