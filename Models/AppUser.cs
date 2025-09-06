using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class AppUser
    {
        [Key]
        public int? Id { get; set; }   // (Recommend: make this int, not nullable, when convenient)

        [Required, StringLength(100)]
        public string Username { get; set; }

        [Required, StringLength(256)]
        public string PasswordHash { get; set; }

        [Required, StringLength(50)]
        public string UserLevel { get; set; }

        [StringLength(150)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(150)]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }
        public DateTime? CreatedDate { get; set; }

        public bool IsDesigner { get; set; }
        public bool IsSalesRep { get; set; }
        public bool IsEstimator { get; set; }
        public bool IsQuoteApprover { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [StringLength(100)]
        public string Department { get; set; }
    }
}
