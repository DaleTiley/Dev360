using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    public class GeneralProjectData
    {
        [Key]
        public int Id { get; set; }
        public string Designer { get; set; }
        public string CustomerName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerId { get; set; }
        public string CustomerDataPath { get; set; }
        public string FinalClientName { get; set; }
        public string SiteLocation { get; set; }
        public string SiteAddress { get; set; }
        public string ProjectName { get; set; }
        public string Fabricator { get; set; }
        public string Template { get; set; }
        public string ProjectType { get; set; }
        public double? DeadLoadRoof { get; set; }
        public double? DeadLoadCeiling { get; set; }
        public string SnowZone { get; set; }
        public double? Altitude { get; set; }
        public string WindZone { get; set; }
        public double? Pitch { get; set; }
        public double? FrameSpacing { get; set; }
        public string HeelType { get; set; }

        //These Are Sub Sections (Column B / 2)
        public double? VerticalHeelOffset { get; set; }
        public double? VerticalHeelHeight { get; set; }

        public double? StandardHeelOffset { get; set; }
        public double? StandardHeelHeight { get; set; }

        public double? FrenchPerpendicularOffset { get; set; }
        public double? FrenchPerpendicularHeight { get; set; }

        public double? ExtendedBottomChordOffset { get; set; }
        public double? ExtendedBottomChordHeight { get; set; }

        public double? WallPlateWidth { get; set; }
        public double? WallPlateLayback { get; set; }

        public double? VATPercentage { get; set; }

        public double? TotalMaterialVolumeTimber { get; set; }
        public double? TotalMaterialVolumeCircularTimber { get; set; }
        public double? TotalMaterialVolumeLVL { get; set; }
        public double? TotalMaterialVolumeGlulam { get; set; }

        public double? TotalMaterialPriceTimber { get; set; }
        public double? TotalMaterialPriceAllWoodenItems { get; set; }
        public double? TotalMaterialPricePlates { get; set; }
        public double? TotalMaterialPriceLVL { get; set; }
        public double? TotalMaterialPriceGlulam { get; set; }
        public double? TotalMaterialPricePosiStruts { get; set; }

        public int Version { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? ProjectId { get; set; }   // nullable is fine to start

    }
}
