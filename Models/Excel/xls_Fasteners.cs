namespace MillenniumWebFixed.Models
{
    public class xls_Fasteners
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Ref { get; set; }
        public string Object { get; set; }
        public string Quantity { get; set; }
        public string Organ_type { get; set; }
        public string Material_name { get; set; }
        public string Manufacturer { get; set; }
        public string Usage { get; set; }
        public string Type { get; set; }
        public string Diameter_mm { get; set; }
        public string Length_mm { get; set; }
        public string Package { get; set; }
        public string Price_Package_R { get; set; }
    }
}