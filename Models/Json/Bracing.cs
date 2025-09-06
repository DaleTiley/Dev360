using MillenniumWebFixed.Models;

public class Bracing : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public double? LENGTH { get; set; }
        public double? STOCK_LENGTH { get; set; }
        public double? THICKNESS { get; set; }
        public double? DEPTH { get; set; }
        public string GRADE { get; set; }
        public string MATERIAL_NAME { get; set; }
        public double? PRICE_M3 { get; set; }
        public string QUOTE_GROUP { get; set; }
}