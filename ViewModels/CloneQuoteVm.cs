using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.ViewModels
{
    public class CloneQuoteVm
    {
        [Required]
        public int SourceQuoteId { get; set; }

        [Required, StringLength(10)]
        [Display(Name = "Building Type")]
        public string BuildingType { get; set; }

        [Display(Name = "Quotation Option")]
        [Range(1, 9999, ErrorMessage = "Enter a number ≥ 1")]
        public int? QuotationOption { get; set; }
    }
}