using MillenniumWebFixed.Models;

public class Metalwork : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public int? COUNT { get; set; }
        public string STATUS { get; set; }
        public string FAILURE_REASON { get; set; }
        public string SUPPLIER { get; set; }
        public string GROUP { get; set; }
        public string HANGER { get; set; }
        public string DESCRIPTION { get; set; }
        public string MATERIAL_NAME { get; set; }
        public double? WIDTH { get; set; }
        public double? HEIGHT { get; set; }
        public double? DEPTH { get; set; }
        public double? FLANGE_WIDTH { get; set; }
        public double? FINAL_PRICE { get; set; }
        public string QUOTE_GROUP { get; set; }
        public string FIXING { get; set; }
        public int? FIXING_QUANTITY { get; set; }
}