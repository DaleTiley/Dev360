
using MillenniumWebFixed.Models;
using System;

public class QuoteHistory
{
    public int Id { get; set; }
    public int QuoteId { get; set; }

    public string Action { get; set; }
    public string PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.Now;
    public string Notes { get; set; }

    public virtual Quote Quote { get; set; }
}
