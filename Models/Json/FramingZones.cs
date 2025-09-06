using MillenniumWebFixed.Models;

public class FramingZones : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public double? SPAN { get; set; }
        public double? PITCH { get; set; }
        public double? PRICE { get; set; }
        public string LAYOUT { get; set; }
        public string STOREY { get; set; }
}