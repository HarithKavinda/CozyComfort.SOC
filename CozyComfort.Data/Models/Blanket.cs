using System;

namespace CozyComfort.Data.Models
{
    public class Blanket
    {
        public int BlanketId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}