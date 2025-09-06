namespace MillenniumWebFixed.Models
{
    public class xls_Bracing
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Length_m { get; set; }
        public string Stocklength_m { get; set; }
        public string Thickness_mm { get; set; }
        public string Depth_mm { get; set; }
        public string Grade { get; set; }
        public string Material_name { get; set; }
        public string Quote_group { get; set; }
        public string Price_R_m3 { get; set; }
    }
}