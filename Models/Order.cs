using System;

namespace ShoeStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public string StatusCode { get; set; } = string.Empty; // Новый, В работе, Выдан
        public string Address { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }
}