using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("prod_ProductAssembly")]

    public class ProductAssembly
    {
        public int Id { get; set; }

        // Explicit FK
        [ForeignKey("Product")]

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public string Name { get; set; }
        public string BaseUOM { get; set; }
        public decimal CoverWidth { get; set; }
        public decimal? OverlapAllowance { get; set; }
        public string Notes { get; set; }

        public virtual ICollection<ProductAssemblyComponent> Components { get; set; }
    }
}