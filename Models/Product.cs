namespace ShoeStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Discount { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        
        // Свойство для отображения имени поставщика (для поиска)
        public string SupplierName => Supplier?.Name ?? "";
        public string CategoryName => Category?.Name ?? "";
    }
}