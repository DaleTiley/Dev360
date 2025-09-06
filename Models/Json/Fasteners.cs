using MillenniumWebFixed.Models;

public class Fasteners : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public string REF { get; set; }
        public string OBJECT { get; set; }
        public int? QUANTITY { get; set; }
        public string TYPE { get; set; }
        public string MATERIAL_NAME { get; set; }
        public string MANUFACTURER { get; set; }
        public string USAGE { get; set; }
        public string SUB_TYPE { get; set; }
        public double? DIAMETER { get; set; }
        public double? LENGTH { get; set; }
        public int? PACKAGE { get; set; }
        public double? PRICE_PER_PACKAGE { get; set; }
}