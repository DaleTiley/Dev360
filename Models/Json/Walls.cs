using MillenniumWebFixed.Models;

public class Walls : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public int? CHAIN { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public string DESCRIPTION { get; set; }
        public bool? SUPPORTING { get; set; }
        public double? LENGTH { get; set; }
        public double? HORIZONTAL_LENGTH { get; set; }
        public int? WALLPLATE_THICKNESS { get; set; }
        public int? WALLPLATE_DEPTH { get; set; }
        public double? HEIGHT_REFERENCE { get; set; }
        public double? HEIGHT_BOTTOM { get; set; }
}