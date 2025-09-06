
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class Quote
    {
        public int Id { get; set; }

        [Required]
        public int ProjectId { get; set; }  // FK to GeneralQuoteDatas

        [Required]
        [MaxLength(50)]
        public string QuoteRef { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsArchived { get; set; }

        public string QuotePath { get; set; }
        public virtual ICollection<QuoteVersion> Versions { get; set; }
    }
}