namespace VendorManagementSystem.Shared.Models;

public class PriceList
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int ProductId { get; set; }

    public decimal Price { get; set; }

    public DateTime EffectiveDate { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
