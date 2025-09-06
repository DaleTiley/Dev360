using MillenniumWebFixed.Models;

public class ManufacturingFrames : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public string NAME { get; set; }
        public int? QUANTITY { get; set; }
        public int? PLIES { get; set; }
        public double? OVERALL_SPAN { get; set; }
        public double? OVERALL_HEIGHT { get; set; }
        public int? ROOF_SEGMENTS { get; set; }
        public double? VOLUME_ACTUAL { get; set; }
        public double? CONNECTOR_AREA { get; set; }
        public int? CONNECTOR_POINTS { get; set; }
        public int? PRODUCTION_SET { get; set; }
        public string LABEL { get; set; }
        public bool? IS_ATTIC { get; set; }
        public bool? IS_ON_CONCRETE { get; set; }
        public string STOREY { get; set; }
        public string LAYOUT { get; set; }
}