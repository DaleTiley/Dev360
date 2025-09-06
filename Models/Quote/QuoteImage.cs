using MillenniumWebFixed.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class QuoteImage
    {
        public int Id { get; set; }

        [Required]
        public int QuoteId { get; set; }

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(500)]
        public string FilePath { get; set; }

        public DateTime? UploadedAt { get; set; }

        // optional nav
        public virtual ManualQuote Quote { get; set; }
    }
}