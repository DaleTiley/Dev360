using System;

namespace MillenniumWebFixed.ViewModels
{
    public class ProjectQuoteRowVm
    {
        public int Id { get; set; }
        public string QuoteNo { get; set; }
        public string Building { get; set; }
        public string Variant { get; set; }
        public string Revision { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
