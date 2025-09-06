using MillenniumWebFixed.Models;

public class PosiStrut : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string NAME { get; set; }
}