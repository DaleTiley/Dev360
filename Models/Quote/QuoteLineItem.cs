
using System.ComponentModel.DataAnnotations;

public class QuoteLineItem
{
    public int Id { get; set; }

    [Required]
    public int QuoteVersionId { get; set; }

    public string JsonItemName { get; set; }
    public int? ProductId { get; set; }
    public string ProductDesc { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? UnitSell { get; set; }
    public string Notes { get; set; }

    public virtual QuoteVersion QuoteVersion { get; set; }
}
