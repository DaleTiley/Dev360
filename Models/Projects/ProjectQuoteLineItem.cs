using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models.Projects
{
    [Table("ProjectQuoteLineItem")]
    public class ProjectQuoteLineItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public int QuoteId { get; set; }

        [StringLength(100)]
        public string ImportedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ImportedDate { get; set; }

        [Column("Quote"), StringLength(100)]
        public string QuoteExcel { get; set; }

        [Column("Quantity")] public decimal? Quantity { get; set; }
        [Column("Select Product"), StringLength(255)]
        public string SelectProduct { get; set; }
        [Column("Price Overridden")] public bool? PriceOverridden { get; set; }
        [Column("Price Per Unit")] public decimal? PricePerUnit { get; set; }
        [Column("Cost Per Unit")] public decimal? CostPerUnit { get; set; }
        [Column("VAT")] public decimal? Vat { get; set; }
        [Column("Product ID"), StringLength(50)]
        public string ProductIdExcel { get; set; }
        [Column("Product Name"), StringLength(255)]
        public string ProductName { get; set; }

        [Column("LineNotes"), DataType(DataType.MultilineText)]
        public string LineNotes { get; set; }

    }
}
