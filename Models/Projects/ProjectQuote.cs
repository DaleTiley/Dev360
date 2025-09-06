using MillenniumWebFixed.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ProjectQuotes")]
public class ProjectQuote
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int QuoteId { get; set; }

    [MaxLength(100)]
    public string QuoteNumber { get; set; }

    public int? EnquiryId { get; set; }
    public int? DesignerUserId { get; set; }

    [MaxLength(50)]
    public string Status { get; set; }

    [Column(TypeName = "decimal")]
    public decimal? TotalAmount { get; set; }

    public int? CreatedByUserId { get; set; }
    public DateTime? CreatedAt { get; set; }

    public int? ProjectId { get; set; }
    public virtual Project Project { get; set; }

    [MaxLength(50)] public string BuildingType { get; set; }
    public int? QuotationOption { get; set; }
    [MaxLength(10)] public string Revision { get; set; }

    public DateTime? SubmissionDate { get; set; }
    public DateTime? LastImportDate { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? QuantityOfUnits { get; set; }
    public int? DesignerId { get; set; }
    [MaxLength(100)] public string Designer { get; set; }

    [MaxLength(50)] public string RoofPitch { get; set; }
    [MaxLength(50)] public string RoofOverhang { get; set; }
    [MaxLength(50)] public string GableOverhang { get; set; }
    [MaxLength(50)] public string RoofCovering { get; set; }
    [MaxLength(50)] public string TcLoading { get; set; }
    [MaxLength(50)] public string BcLoading { get; set; }
    [MaxLength(50)] public string WindSpeed { get; set; }
    [MaxLength(50)] public string TerrainCategory { get; set; }
    [MaxLength(50)] public string MaxBattensCc { get; set; }
    [MaxLength(50)] public string MaxTrussesCc { get; set; }

    [Column(TypeName = "decimal")] public decimal? RoofArea { get; set; }
    [Column(TypeName = "decimal")] public decimal? FloorArea { get; set; }

    [MaxLength(100)] public string PmrjbFileRef { get; set; }
    [MaxLength(2000)] public string Notes { get; set; }

    public string SubmittedForApprovalBy { get; set; }   // username/fullname
    public DateTime? SubmittedForApprovalOn { get; set; } // UTC
    public string SubmittedForApprovalTo { get; set; }   // email list "a@x;b@y"
    public string ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
}
