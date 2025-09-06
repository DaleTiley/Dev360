
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("ref_UOM")]
    public class UOM
    {
        [Key]
        public string UOMCode { get; set; }
        public string Description { get; set; }
        public string BaseUnit { get; set; }
    }
}