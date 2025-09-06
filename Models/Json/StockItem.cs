using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class StockItem
    {
        [Key]
        public int Id { get; set; }

        public string ProductCode { get; set; }
        public string Description { get; set; }
        public string Units { get; set; }
        public double? WeightKg { get; set; }
        public double? ThicknessMM { get; set; }
        public double? WidthMM { get; set; }
        public double? LengthMM { get; set; }
        public double? DiameterMM { get; set; }
        public double? HeightDepthMM { get; set; }
        public double? AreaM2 { get; set; }
        public double? SurfaceAreaM2 { get; set; }
        public double? VolumeM3 { get; set; }
        public string MaterialType { get; set; }
        public string VisualGrade { get; set; }
        public string StrengthGrade { get; set; }
        public string Class { get; set; }
        public string Colour { get; set; }
        public string Manufacturer { get; set; }
        public string Notes { get; set; }
        public string WasEnriched { get; set; }
        public string ImageUrl { get; set; }
        public string InstallGuideUrl { get; set; }
        public string ScrapedSpecs { get; set; }
    }
}
