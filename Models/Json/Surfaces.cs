using MillenniumWebFixed.Models;

public class Surfaces : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public int? CHAIN { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public string PITCH { get; set; }
        public double? AREA { get; set; }
        public double? AREA_OUTSIDE_WALLS { get; set; }
        public bool? GABLE { get; set; }
}