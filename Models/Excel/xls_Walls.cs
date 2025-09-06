namespace MillenniumWebFixed.Models
{
    public class xls_Walls
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Chain { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string Supporting { get; set; }
        public string Length_m { get; set; }
        public string Horizontal_length_m { get; set; }
        public string Wallplate_thickness_mm { get; set; }
        public string Wallplate_depth_mm { get; set; }
        public string Height_reference_mm { get; set; }
        public string Height_bottom_mm { get; set; }
    }
}