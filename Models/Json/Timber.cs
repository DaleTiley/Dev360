using MillenniumWebFixed.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Timber")]

public class Timber : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string FRAME { get; set; }
    public int? QUANTITY { get; set; }
    public int? QUANTITY_PER_PLY { get; set; }
    public string LABEL { get; set; }
    public string TYPE { get; set; }
    public double? OVERALL_LENGTH { get; set; }
    public double? THICKNESS { get; set; }
    public double? DEPTH { get; set; }
    public string GRADE { get; set; }
    public string PRODUCT_CODE { get; set; }
    public string MATERIAL_NAME { get; set; }
    public double? PRICE_M3 { get; set; }
    public int? PART_NUMBER { get; set; }
    public int? MEMBER_LAYERS { get; set; }
}