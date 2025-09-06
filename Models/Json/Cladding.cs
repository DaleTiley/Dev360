using MillenniumWebFixed.Models;

public class Cladding : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string CHAIN { get; set; }
    public string NAME { get; set; }
    public string TYPE { get; set; }
    public string PITCH { get; set; }
    public string AREA { get; set; }
}