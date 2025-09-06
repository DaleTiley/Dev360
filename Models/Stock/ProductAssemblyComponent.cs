using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("prod_ProductAssemblyComponent")]
    public class ProductAssemblyComponent
    {
        public int Id { get; set; }
        public int AssemblyId { get; set; }
        public int ComponentProductId { get; set; }
        public string Type { get; set; }
        public decimal QtyPerUOM { get; set; }
        public string Unit { get; set; }
        public decimal CostPerUnit { get; set; }
        public string TotalFormula { get; set; }
        public string Notes { get; set; }

        public ProductAssembly Assembly { get; set; }

        [ForeignKey("ComponentProductId")]
        public virtual Product ComponentProduct { get; set; }
    }
}