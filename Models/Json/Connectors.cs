using MillenniumWebFixed.Models;

public class Connectors : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public string FRAME { get; set; }
        public int? COUNT { get; set; }
        public int? COUNT_PER_PLY { get; set; }
        public double? DEPTH { get; set; }
        public double? LENGTH { get; set; }
        public string GAUGE { get; set; }
        public string MATERIAL_NAME { get; set; }
        public string PRODUCT_CODE { get; set; }
        public double? PRICE_M2 { get; set; }
        public int? PART_NUMBER { get; set; }
}