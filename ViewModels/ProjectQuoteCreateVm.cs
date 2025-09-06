using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MillenniumWebFixed.ViewModels
{
    public class ProjectQuoteCreateVm
    {
        public int QuoteId { get; set; }

        [Required] public int ProjectId { get; set; }

        [Display(Name = "Quote No")]
        public string QuoteNumber { get; set; } // readonly, generated

        [Required, StringLength(50)]
        [Display(Name = "Building Type")]
        public string BuildingType { get; set; }

        [Required(ErrorMessage = "Quotation Option is required")]
        [Range(1, 99, ErrorMessage = "Quotation Option must be a number between 1 and 99")]
        [Display(Name = "Quotation Option")]
        public int? QuotationOption { get; set; }

        [Required(ErrorMessage = "Revision is required")]
        [RegularExpression(@"^([A-Za-z]|\d+)$", ErrorMessage = "Revision must be a single letter (A–Z) or a number")]
        [Display(Name = "Revision")]
        public string Revision { get; set; } = "A";


        [Range(1, int.MaxValue, ErrorMessage = "Enter at least 1 unit.")]
        [Display(Name = "Quantity of Units")]
        public int? QuantityOfUnits { get; set; }

        [StringLength(100)]
        public string Designer { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Submission Date")]
        public DateTime? SubmissionDate { get; set; }

        [StringLength(100)]
        [Display(Name = "PMRJB File Ref")]
        public string PmrjbFileRef { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Last Import Date")]
        public DateTime? LastImportDate { get; set; }

        // Design details (keep light for now)
        [StringLength(50)] public string RoofPitch { get; set; }
        [StringLength(50)] public string RoofOverhang { get; set; }
        [StringLength(50)] public string GableOverhang { get; set; }
        [StringLength(50)] public string RoofCovering { get; set; }
        [StringLength(50), Display(Name = "TC Loading")] public string TcLoading { get; set; }
        [StringLength(50), Display(Name = "BC Loading")] public string BcLoading { get; set; }
        [StringLength(50)] public string WindSpeed { get; set; }
        [StringLength(50), Display(Name = "Terrain Category")] public string TerrainCategory { get; set; }
        [StringLength(50), Display(Name = "Max Battens C/C")] public string MaxBattensCc { get; set; }
        [StringLength(50), Display(Name = "Max Trusses C/C")] public string MaxTrussesCc { get; set; }

        [Display(Name = "Roof Area (m²)")]
        [RegularExpression(@"^\s*\d+([.,]\d{1,2})?\s*$", ErrorMessage = "Use digits with optional . or , and up to 2 decimals")]
        public string RoofArea { get; set; }

        [Display(Name = "Floor Area (m²)")]
        [RegularExpression(@"^\s*\d+([.,]\d{1,2})?\s*$", ErrorMessage = "Use digits with optional . or , and up to 2 decimals")]
        public string FloorArea { get; set; }

        [StringLength(2000)]
        [Display(Name = "Quote Notes / Special")]
        public string Notes { get; set; }

        // DB-backed fields already handled elsewhere:
        public string Status { get; set; } = "Draft";
        public decimal? TotalAmount { get; set; }

        public int? DesignerId { get; set; }
        public IEnumerable<SelectListItem> DesignerOptions { get; set; } = new List<SelectListItem>();

        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}