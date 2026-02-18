namespace CozyComfort.Data.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public int? FromUserId { get; set; }
        public int? ToUserId { get; set; }
        public string OrderType { get; set; } = "SellerToDistributor";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
