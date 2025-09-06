using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("ref_ItemType")]
    public class ItemType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BaseUOM { get; set; }
        public bool SupportsM2 { get; set; }
        public bool SupportsM3 { get; set; }
        public string Notes { get; set; }

        public virtual ICollection<ItemTypeProperty> Properties { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<UOMConversion> UOMConversions { get; set; }
    }
}