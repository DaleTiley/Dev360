using MillenniumWebFixed.Models;

public class Sheeting : IGeneralProjectDataBound
{
    public int? Id { get; set; }
    public int? GeneralProjectDataId { get; set; }
        public int? QUANTITY { get; set; }
        public int? LABEL { get; set; }
        public string MATERIAL { get; set; }
        public string PRODUCT_CODE { get; set; }
        public double? THICKNESS { get; set; }
        public double? WIDTH { get; set; }
        public double? LENGTH { get; set; }
        public double? WEIGHT { get; set; }
        public double? PRICE { get; set; }
}