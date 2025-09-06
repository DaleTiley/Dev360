using MillenniumWebFixed.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Co2Materials")]

public class Co2Material : IGeneralProjectDataBound
{
    [Key]
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    [Column("CO2_ID")]
    public string ID { get; set; }
    public string MATERIAL { get; set; }
    public string SUBCATEGORY { get; set; }
    public bool? SITEFIXED { get; set; }
    public double? MASS { get; set; }
    public double? VOLUME { get; set; }
    public string AREA { get; set; }
    public string LENGTH { get; set; }
    public double? RESULTVALUE { get; set; }
    public string UNIT { get; set; }
}