namespace MillenniumWebFixed.Models
{
    public class xls_Surfaces
    {
        public int Id { get; set; }
        public int GeneralProjectDataId { get; set; }
        public string Chain { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Pitch_degree { get; set; }
        public string Area_m2 { get; set; }
        public string Area_outside_walls_m2 { get; set; }
        public string Gable { get; set; }
    }
}