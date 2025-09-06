using Newtonsoft.Json;
using System.Collections.Generic;

namespace MillenniumWebFixed.Models
{
    public class RootObject
    {
        public List<ProjectSection> ProjectData { get; set; }
    }

    public class ProjectSection
    {
        public List<GeneralQuoteData> GeneralData { get; set; }

        [JsonProperty("FRAMING_ZONES_TABLE")]
        public List<FramingZones> FramingZonesTable { get; set; }

        [JsonProperty("FRAMES_TABLE")]
        public List<Frames> FramesTable { get; set; }

        [JsonProperty("MANUFACTURING_FRAMES_TABLE")]
        public List<ManufacturingFrames> ManufacturingFramesTable { get; set; }

        [JsonProperty("TIMBER_TABLE")]
        public List<Timber> TimberTable { get; set; }

        [JsonProperty("CONNECTORS_TABLE")]
        public List<Connectors> ConnectorsTable { get; set; }

        [JsonProperty("METALWORK_TABLE")]
        public List<Metalwork> MetalworkTable { get; set; }

        [JsonProperty("BRACING_TABLE")]
        public List<Bracing> BracingTable { get; set; }

        [JsonProperty("WALLS_TABLE")]
        public List<Walls> WallsTable { get; set; }

        [JsonProperty("SURFACES_TABLE")]
        public List<Surfaces> SurfacesTable { get; set; }

        [JsonProperty("ROOFING_DATA_TABLE")]
        public List<RoofingData> RoofingDataTable { get; set; }

        [JsonProperty("FASTENERS_TABLE")]
        public List<Fasteners> FastenersTable { get; set; }

        [JsonProperty("SHEETING_TABLE")]
        public List<Sheeting> SheetingTable { get; set; }

        [JsonProperty("CO2_MATERIAL_TABLE")]
        public List<Co2Material> Co2MaterialTable { get; set; }

        // Optional sections
        [JsonProperty("ATTIC_ROOMS_TABLE")]
        public List<AtticRoom> AtticRoomsTable { get; set; }

        [JsonProperty("BOARDS_TABLE")]
        public List<Board> BoardsTable { get; set; }

        [JsonProperty("CLADDING_TABLE")]
        public List<Cladding> CladdingTable { get; set; }

        [JsonProperty("DECKING_TABLE")]
        public List<Decking> DeckingTable { get; set; }

        [JsonProperty("OPENING_FRAME_LABEL")]
        public List<OpeningFrameLabel> OpeningFrameLabelTable { get; set; }

        [JsonProperty("POSI_STRUT_TABLE")]
        public List<PosiStrut> PosiStrutTable { get; set; }

        [JsonProperty("SUNDRY_ITEMS_TABLE")]
        public List<SundryItem> SundryItemsTable { get; set; }
    }
}
