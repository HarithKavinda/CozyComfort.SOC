using System.ComponentModel.DataAnnotations;

namespace CozyComfort.Data.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public int? OwnerUserId { get; set; }
        public string OwnerRole { get; set; } = "Manufacturer";
        public int Id { get; set; }
    }
}
