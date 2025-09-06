using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models
{
    [Table("ProjectVersions")] // maps to dbo.ProjectVersions
    public class ProjectVersion
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }       // FK → Projects.Id
        public int VersionNo { get; set; }
        public DateTime CreatedAt { get; set; }

        // (optional) nav prop if you like
        // public virtual Project Project { get; set; }
    }
}
