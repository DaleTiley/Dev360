using MillenniumWebFixed.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("RoofingData")]

public class RoofingData : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string TYPE { get; set; }
    public double? LENGTH { get; set; }
}