
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MillenniumWebFixed.Models
{
    /*********************************************
        THIS IS USED FOR JSON FILE IMPORTS
    **********************************************/
    public class GeneralQuoteData
    {
        [Key]
        public int? Id { get; set; }

        // Basic Info
        [StringLength(100)]
        [JsonProperty("DESIGNER")]
        public string Designer { get; set; }

        // Customer (Client)
        [StringLength(100)]
        [JsonProperty("CUSTOMER_NAME")]
        public string CustomerName { get; set; }

        [StringLength(250)]
        [JsonProperty("CUSTOMER_ADDRESS")]
        public string CustomerAddress { get; set; }

        [StringLength(50)]
        [JsonProperty("CUSTOMER_ID")]
        public string CustomerId { get; set; }

        [StringLength(250)]
        [JsonProperty("CUSTOMER_DATA_PATH")]
        public string CustomerDataPath { get; set; }

        // Final Client (optional)
        [StringLength(100)]
        [JsonProperty("FINAL_CLIENT_NAME")]
        public string FinalClientName { get; set; }

        // Site
        [StringLength(100)]
        [JsonProperty("SITE_LOCATION")]
        public string SiteLocation { get; set; }

        [StringLength(250)]
        [JsonProperty("SITE_ADDRESS")]
        public string SiteAddress { get; set; }

        // Project
        [StringLength(150)]
        [JsonProperty("PROJECT_NAME")]
        public string ProjectName { get; set; }

        [StringLength(100)]
        [JsonProperty("FABRICATOR")]
        public string Fabricator { get; set; }

        [StringLength(100)]
        [JsonProperty("TEMPLATE")]
        public string Template { get; set; }

        [StringLength(50)]
        [JsonProperty("PROJECT_TYPE")]
        public string ProjectType { get; set; }

        // Load Specifications
        [JsonProperty("DEAD_LOAD_ROOF")]
        public double? DeadLoadRoof { get; set; }

        [JsonProperty("DEAD_LOAD_CEILING")]
        public double? DeadLoadCeiling { get; set; }

        [StringLength(50)]
        [JsonProperty("SNOW_ZONE")]
        public string SnowZone { get; set; }

        [JsonProperty("ALTITUDE")]
        public double? Altitude { get; set; }

        [StringLength(50)]
        [JsonProperty("WIND_ZONE")]
        public string WindZone { get; set; }

        [JsonProperty("PITCH")]
        public double? Pitch { get; set; }

        [JsonProperty("FRAME_SPACING")]
        public double? FrameSpacing { get; set; }

        [StringLength(50)]
        [JsonProperty("HEEL_TYPE")]
        public string HeelType { get; set; }

        // Heel Dimensions
        [JsonProperty("VERTICAL_HEEL_OFFSET")]
        public double? VerticalHeelOffset { get; set; }

        [JsonProperty("VERTICAL_HEEL_HEIGHT")]
        public double? VerticalHeelHeight { get; set; }

        [JsonProperty("STANDARD_HEEL_OFFSET")]
        public double? StandardHeelOffset { get; set; }

        [JsonProperty("STANDARD_HEEL_HEIGHT")]
        public double? StandardHeelHeight { get; set; }

        [JsonProperty("FRENCH_PERPENDICULAR_OFFSET")]
        public double? FrenchPerpendicularOffset { get; set; }

        [JsonProperty("FRENCH_PERPENDICULAR_HEIGHT")]
        public double? FrenchPerpendicularHeight { get; set; }

        [JsonProperty("EXTENDED_BOTTOM_CHORD_OFFSET")]
        public double? ExtendedBottomChordOffset { get; set; }

        [JsonProperty("EXTENDED_BOTTOM_CHORD_HEIGHT")]
        public double? ExtendedBottomChordHeight { get; set; }

        // Wall Plate
        [JsonProperty("WALL_PLATE_WIDTH")]
        public double? WallPlateWidth { get; set; }

        [JsonProperty("WALL_PLATE_LAYBACK")]
        public double? WallPlateLayback { get; set; }

        // Financial
        [JsonProperty("VAT_PERCENTAGE")]
        public double? VATPercentage { get; set; }

        // Material Volumes
        [JsonProperty("TOTAL_MATERIAL_VOLUME_TIMBER")]
        public double? TotalMaterialVolumeTimber { get; set; }

        [JsonProperty("TOTAL_MATERIAL_VOLUME_CIRCULAR_TIMBER")]
        public double? TotalMaterialVolumeCircularTimber { get; set; }

        [JsonProperty("TOTAL_MATERIAL_VOLUME_LVL")]
        public double? TotalMaterialVolumeLVL { get; set; }

        [JsonProperty("TOTAL_MATERIAL_VOLUME_GLULAM")]
        public double? TotalMaterialVolumeGlulam { get; set; }

        // Material Prices
        [JsonProperty("MATERIAL_TOTAL_TIMBER_PRICE")]
        public double? TotalMaterialPriceTimber { get; set; }

        //[JsonProperty("TOTAL_MATERIAL_PRICE_LVL")]
        //public double? TotalMaterialPriceLVL { get; set; }

        //[JsonProperty("TOTAL_MATERIAL_PRICE_GLULAM")]
        //public double? TotalMaterialPriceGlulam { get; set; }

        [JsonProperty("MATERIAL_TOTAL_ALLWOODENITEMS_PRICE")]
        public double? TotalMaterialPriceAllWoodenItems { get; set; }

        [JsonProperty("MATERIAL_TOTAL_PLATES_PRICE")]
        public double? TotalMaterialPricePlates { get; set; }

        public int Version { get; set; } = 0;

        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }
    }
}
