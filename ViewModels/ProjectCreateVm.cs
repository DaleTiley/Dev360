using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;


namespace MillenniumWebFixed.Models
{
    public class ProjectCreateVm
    {
        public int? SalesRepId { get; set; }
        public IEnumerable<SelectListItem> SalesRepOptions { get; set; } = new List<SelectListItem>();

        // --- Core ---
        [Required, StringLength(100)]
        [Display(Name = "Project Code")]
        public string ProjectCode { get; set; }

        // Selected client (lookup)
        [Required(ErrorMessage = "Please select a client")]
        [Display(Name = "Client")]
        [HiddenInput(DisplayValue = false)]
        public int? ClientId { get; set; }     // stored, but DB doesn't yet have FK (future)

        [StringLength(200)]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } // filled by modal, readonly in UI

        [StringLength(200)]
        [Display(Name = "Site Name (optional)")]
        public string SiteName { get; set; }

        // --- Primary Contact (lookup) ---
        [HiddenInput(DisplayValue = false)]
        public int? ContactId { get; set; }

        [StringLength(100)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [StringLength(150)]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [StringLength(50)]
        [RegularExpression(@"^[-0-9 ()+]{7,}$", ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Contact Phone")]
        public string ContactPhone { get; set; }

        // --- Address ---
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string SiteAddress1 { get; set; }

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
        public string SiteAddress2 { get; set; }

        [StringLength(100)]
        [Display(Name = "City/Town")]
        public string SiteCity { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string SitePostcode { get; set; }

        [StringLength(100)]
        [Display(Name = "Province/State")]
        public string SiteProvince { get; set; }

        // --- NEW: Legal/Location/Links ---
        [StringLength(50)]
        [Display(Name = "ERP Number")]
        public string ERPNumber { get; set; }

        [StringLength(100)]
        [Display(Name = "Township")]
        public string Township { get; set; }

        [StringLength(50)]
        [Display(Name = "Portion No.")]
        public string PortionNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Stand No.")]
        public string StandNumber { get; set; }

        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [StringLength(500)]
        [Url]
        [Display(Name = "Google Map URL")]
        public string GoogleMapUrl { get; set; }

        [StringLength(500)]
        [Url]
        [Display(Name = "SharePoint URL")]
        public string SharePointUrl { get; set; }

        [StringLength(100)]
        [Display(Name = "Site Rentals")]
        public string SiteRentals { get; set; }

        // --- NEW: People/CRM ---
        [StringLength(120)]
        [Display(Name = "Sales Person")]
        public string SalesPerson { get; set; }

        [StringLength(120)]
        [Display(Name = "Site Contact Name")]
        public string SiteContactName { get; set; }

        [StringLength(50)]
        [Display(Name = "Site Contact Phone")]
        public string SiteContactPhone { get; set; }

        [StringLength(50)]
        [Display(Name = "CRM Stage")]
        public string CrmStage { get; set; }   // Lead/Quoted/Won/Lost

        [StringLength(120)]
        [Display(Name = "Next Action")]
        public string CrmNextAction { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Follow-up Date")]
        public DateTime? CrmFollowUpDate { get; set; }

        // --- Notes / Files ---
        [DataType(DataType.MultilineText)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Attach plans or documents")]
        public HttpPostedFileBase[] Files { get; set; }

        public string CreatedBy { get; set; }            // nvarchar(100)
        public int? CreatedByUserId { get; set; }        // optional
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
