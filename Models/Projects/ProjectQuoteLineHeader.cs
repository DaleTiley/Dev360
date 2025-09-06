using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MillenniumWebFixed.Models.Projects
{
    [Table("ProjectQuoteLineHeader")]
    public class ProjectQuoteLineHeader
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public int QuoteId { get; set; }

        [StringLength(100)]
        public string ImportedBy { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime ImportedDate { get; set; }

        [Column("Quote ID"), StringLength(100)]
        public string QuoteExcel { get; set; }

        [Column("Roof Pitch")] public decimal? RoofPitch { get; set; }
        [Column("Roof Overhang")] public decimal? RoofOverhang { get; set; }
        [Column("Roof Gable Overhang")] public decimal? RoofGableOverhang { get; set; }

        [Column("Roof Covering"), StringLength(100)]
        public string RoofCovering { get; set; }

        [Column("Max Batten Centres")] public decimal? MaxBattenCentres { get; set; }
        [Column("Max Truss Centres")] public decimal? MaxTrussCentres { get; set; }
        [Column("Floor Area")] public decimal? FloorArea { get; set; }
        [Column("Roof Area")] public decimal? RoofArea { get; set; }
        [Column("E-Finks (Work Units)")] public decimal? EFinksWorkUnits { get; set; }
        [Column("E-Finks Cost")] public decimal? EFinksCost { get; set; }
        [Column("Transport Cost")] public decimal? TransportCost { get; set; }
    }
}
