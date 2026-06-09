namespace App.Shared.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Price { get; set; }
        public string? Photo { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public decimal Discount { get; set; }
        public string Unit { get; set; } = "пара";

        public decimal FinalPrice => Price - (Price * Discount / 100);
        public bool IsInStock => Count > 0;
        public bool HasHighDiscount => Discount > 15;
    }

    public class ProductFilter
    {
        public string? SearchText { get; set; }
        public int? SupplierId { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}