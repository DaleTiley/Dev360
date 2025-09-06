using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("ref_UOMConversion")]
    public class UOMConversion
    {
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public string FromUOM { get; set; }
        public string ToUOM { get; set; }
        public string Formula { get; set; }
        public bool RequiresProperties { get; set; }
        public string Notes { get; set; }

        public ItemType ItemType { get; set; }
    }
}