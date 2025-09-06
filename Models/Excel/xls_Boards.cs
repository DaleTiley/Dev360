namespace MillenniumWebFixed.Models
{
    public class xls_Boards
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Quantity { get; set; }
        public string Type { get; set; }
        public string Board_name { get; set; }
        public string Product_code { get; set; }
        public string Thickness_mm { get; set; }
        public string Width_mm { get; set; }
        public string Length_mm { get; set; }
        public string Weight_kg { get; set; }
        public string Price_R { get; set; }
    }
}