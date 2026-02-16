using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.DTOs.Ratings;

public class VendorRatingDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string RaterId { get; set; } = string.Empty;
    public string RaterName { get; set; } = string.Empty;
    public double OverallRating { get; set; }
    public double QualityScore { get; set; }
    public double PriceScore { get; set; }
    public double DeliveryScore { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateVendorRatingRequest
{
    [Required]
    public int SupplierId { get; set; }

    [Required]
    public int PurchaseOrderId { get; set; }

    [Range(0, 5)]
    public double QualityScore { get; set; }

    [Range(0, 5)]
    public double PriceScore { get; set; }

    [Range(0, 5)]
    public double DeliveryScore { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}

public class RatingCriteriaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Weight { get; set; }
}
