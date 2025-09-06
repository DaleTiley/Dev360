using MillenniumWebFixed.Models;
using System;
using System.ComponentModel.DataAnnotations;

public class ProjectImage : IGeneralProjectDataBound
{
    public int? Id { get; set; }

    [Required]
    public int? GeneralProjectDataId { get; set; }

    [Required]
    public string FileName { get; set; }

    public string FilePath { get; set; }  //Just this is enough

    public DateTime? UploadedAt { get; set; } = DateTime.UtcNow;
}
