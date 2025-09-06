using MillenniumWebFixed.Models;

public class AtticRoom : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string NAME { get; set; }
}