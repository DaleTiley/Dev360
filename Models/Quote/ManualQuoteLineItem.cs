using MillenniumWebFixed.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    public class ManualQuoteLineItem
    {
        public int Id { get; set; }

        public int ManualQuoteId { get; set; }
        [ForeignKey("ManualQuoteId")]
        public virtual ManualQuote ManualQuote { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        public int Qty { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? MarginPercent { get; set; }
        public decimal? SellingPrice { get; set; }
    }

    //public class ManualQuoteLineItem
    //{
    //    public int Id { get; set; }

    //    public int ManualQuoteId { get; set; }
    //    [ForeignKey("ManualQuoteId")]
    //    public virtual ManualQuote ManualQuote { get; set; }

    //    public int ProductId { get; set; }
    //    [ForeignKey("ProductId")]
    //    public virtual Product Product { get; set; }

    //    public int Qty { get; set; }
    //    public decimal? CostPrice { get; set; }
    //    public decimal? MarginPercent { get; set; }
    //    public decimal? SellingPrice { get; set; }

    //    [NotMapped]
    //    public string ProductCode { get; set; }          // maps from JsonItemCode

    //    [NotMapped]
    //    public string ProductDescription { get; set; }   // maps from ProductDesc

    //    [NotMapped]
    //    public string BaseUOM { get; set; }              // maps from Unit

    //}

}