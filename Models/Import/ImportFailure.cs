using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class ImportFailure
    {
        public int? Id { get; set; }

        [Required]
        public string ProjectId { get; set; }

        [Required]
        public string Section { get; set; }

        [Required]
        public string ErrorMessage { get; set; }

        public DateTime? TimeStamp { get; set; } = DateTime.Now;
    }
}
