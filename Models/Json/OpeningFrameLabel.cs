using MillenniumWebFixed.Models;

public class OpeningFrameLabel : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string NAME { get; set; }
}