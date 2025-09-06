namespace MillenniumWebFixed.ViewModels
{
    public class QuoteLineImportDto
    {
        public string Section { get; set; }
        public string Group { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string Uom { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitSell { get; set; }
        public int? SortOrder { get; set; }
    }
}
