using System.ComponentModel.DataAnnotations;

namespace MillinniumWebFixed.Models
{
    public class User
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string UserLevel { get; set; } // Admin, Standard, ReadOnly
    }
}
