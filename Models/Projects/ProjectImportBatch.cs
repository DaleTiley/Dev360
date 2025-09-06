using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("ProjectImportBatch")] // maps to dbo.ProjectImportBatch (singular)
    public class ProjectImportBatch
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }           // FK → Projects.Id
        public int ProjectVersionId { get; set; }    // FK → ProjectVersions.Id

        [StringLength(255)]
        public string OriginalFileName { get; set; }

        [Required, StringLength(64)]
        public string FileHash { get; set; }         // SHA-256 hex

        [StringLength(100)]
        public string ImportedBy { get; set; }

        public DateTime ImportedAt { get; set; }
    }
}
