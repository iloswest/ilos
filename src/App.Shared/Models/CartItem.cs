using App.Shared.Models;

namespace App.Shared.Models
{
    public class CartItem
    {
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => (Product?.FinalPrice ?? 0) * Quantity;
    }
}
