using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("prod_ProductMappings")]
    public class ProductMapping
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "JSON Item Name")]
        public string JsonItemName { get; set; }

        [Required]
        [Display(Name = "Mapped Product Name")]
        public string ProductDesc { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
