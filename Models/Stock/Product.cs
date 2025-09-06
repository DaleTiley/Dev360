using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("prod_Product")]
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProductCode { get; set; }
        public int ItemTypeId { get; set; }
        public string BaseUOM { get; set; }
        public bool IsActive { get; set; }

        public int? StockLevel { get; set; }
        public int? ReorderPoint { get; set; }
        public int? LeadTimeDays { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public string Notes { get; set; }

        public ItemType ItemType { get; set; }
        public virtual ICollection<ProductProperty> Properties { get; set; }
        public virtual ICollection<ProductAssembly> Assemblies { get; set; }

    }
}
