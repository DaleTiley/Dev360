using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models.Projects
{
    [Table("QuoteWorkflowLog")]

    public class QuoteWorkflowLog
    {
        public int Id { get; set; }
        public int QuoteId { get; set; }
        public string Event { get; set; }          // "Submit","Notify","Approve","Reject","Cancel"
        public int? ActorUserId { get; set; }
        public string ActorName { get; set; }
        public string ActorEmail { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string MetaJson { get; set; }
    }

}