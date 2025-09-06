namespace MillenniumWebFixed.Models
{
    public class xls_Decking
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Quantity { get; set; }
        public string Label { get; set; }
        public string Material { get; set; }
        public string Product_code { get; set; }
        public string Thickness_mm { get; set; }
        public string Width_mm { get; set; }
        public string Length_mm { get; set; }
        public string Cut_Width_mm { get; set; }
        public string Cut_Length_mm { get; set; }
        public string Weight_kg { get; set; }
        public string Price_R { get; set; }
    }
}