
using MillenniumWebFixed.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class QuoteVersion
{
    public int Id { get; set; }

    [Required]
    public int QuoteId { get; set; }

    public int VersionNumber { get; set; }
    public string Status { get; set; }
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual Quote Quote { get; set; }
    public virtual ICollection<QuoteLineItem> LineItems { get; set; }
    public virtual ICollection<QuoteAttachment> Attachments { get; set; }
}
