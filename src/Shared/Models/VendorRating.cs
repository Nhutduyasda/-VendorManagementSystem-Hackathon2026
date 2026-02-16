using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class VendorRating
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    public int PurchaseOrderId { get; set; }

    [Required]
    public string RaterId { get; set; } = string.Empty;

    public double OverallRating { get; set; }

    public double QualityScore { get; set; }

    public double PriceScore { get; set; }

    public double DeliveryScore { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}
