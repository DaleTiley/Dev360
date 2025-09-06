namespace MillenniumWebFixed.Controllers.Projects
{
    public class QuoteHeaderPatch
    {
        public string RoofPitch { get; set; }                 // keep as string, e.g. "15°" or "15"
        public decimal? RoofOverhang { get; set; }
        public decimal? GableOverhang { get; set; }           // “Roof Gable Overhang” in Excel
        public string RoofCovering { get; set; }
        public decimal? MaxBattensCc { get; set; }            // “Max Batten Centres”
        public decimal? MaxTrussesCc { get; set; }            // “Max Truss Centres”
        public decimal? FloorArea { get; set; }
        public decimal? RoofArea { get; set; }
    }
}