using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int MinStock { get; set; }

    public int MaxStock { get; set; }

    public int CurrentStock { get; set; }

    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;
    public ICollection<SupplierProduct> SupplierProducts { get; set; } = [];
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = [];
}
