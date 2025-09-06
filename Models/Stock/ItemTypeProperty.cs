using MillenniumWebFixed.Models;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ref_ItemTypeProperty")]
public class ItemTypeProperty
{
    public int Id { get; set; }
    public int ItemTypeId { get; set; }
    public string PropertyName { get; set; }
    public string DataType { get; set; }
    public string Units { get; set; }
    public bool IsCalculated { get; set; }
    public string Formula { get; set; }

    public ItemType ItemType { get; set; }
}
