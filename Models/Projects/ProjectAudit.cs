// Models/ProjectAudit.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class ProjectAudit
    {
        public int Id { get; set; }

        [Required] public int ProjectId { get; set; }

        [Required, StringLength(30)]
        public string Area { get; set; }   // "General", "Quote", ...

        [Required, StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public int ChangedByUserId { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; }

        public string MetaJson { get; set; } // optional structured data
    }

    public static class AuditArea
    {
        public const string General = "General";
        public const string Quote = "Quote";
        public const string Tender = "Tender";
        public const string Order = "Order";
        public const string Sales = "Sales";
        public const string Attachments = "Attachments";
    }
}
