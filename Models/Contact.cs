using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(200)]
        public string Email { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Optional: add navigation property
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
}
