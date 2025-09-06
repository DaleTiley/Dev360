using System.ComponentModel.DataAnnotations.Schema;
namespace MillenniumWebFixed.Models
{
    [Table("prod_ProductProperty")]
    public class ProductProperty
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
        public string Units { get; set; }

        public Product Product { get; set; }
    }
}