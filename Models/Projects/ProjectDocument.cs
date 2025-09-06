using System;

namespace MillenniumWebFixed.Models
{
    public class ProjectDocument
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string OriginalFileName { get; set; }
        public string StoredPath { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
