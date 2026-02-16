namespace VendorManagementSystem.Shared.Models;

public class SupplierProduct
{
    public int SupplierId { get; set; }

    public int ProductId { get; set; }

    public decimal Price { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
