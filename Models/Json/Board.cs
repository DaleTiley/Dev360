using MillenniumWebFixed.Models;

public class Board : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
    public string FRAME { get; set; }
    public string QUANTITY { get; set; }
    public string QUANTITY_PER_PLY { get; set; }
    public string LABEL { get; set; }
    public string TYPE { get; set; }
}