using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.DTOs.Vendors;

public class SupplierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxCode { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? LogoUrl { get; set; }
    public SupplierStatus Status { get; set; }
    public SupplierRank Rank { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? BlacklistReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int ContractCount { get; set; }
    public int ProductCount { get; set; }
    public double? AverageRating { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
}

public class SupplierListResponse
{
    public List<SupplierDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class SupplierOverviewDto
{
    public SupplierDto Supplier { get; set; } = new();
    public List<SupplierProductDto> Products { get; set; } = [];
    public List<PriceListDto> PriceHistory { get; set; } = [];
    public List<SupplierRatingDto> Ratings { get; set; } = [];
    public SupplierStatsDto Stats { get; set; } = new();
}

public class SupplierStatsDto
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int ActiveContracts { get; set; }
    public double AverageDeliveryScore { get; set; }
    public double AverageQualityScore { get; set; }
    public double AveragePriceScore { get; set; }
}

public class SupplierRatingDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string RaterId { get; set; } = string.Empty;
    public double OverallRating { get; set; }
    public double QualityScore { get; set; }
    public double PriceScore { get; set; }
    public double DeliveryScore { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PriceComparisonDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public List<SupplierPriceDto> SupplierPrices { get; set; } = [];
}

public class SupplierPriceDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsCheapest { get; set; }
}

public class BlacklistRequest
{
    [Required, MaxLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public class LinkSupplierProductsRequest
{
    public List<SupplierProductItemDto> Products { get; set; } = [];
}

public class SupplierProductItemDto
{
    public int ProductId { get; set; }
    public decimal Price { get; set; }
}

public class CreateSupplierRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TaxCode { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    public string? LogoUrl { get; set; }
}

public class UpdateSupplierRequest : CreateSupplierRequest
{
    public int Id { get; set; }
    public SupplierStatus Status { get; set; }
    public SupplierRank Rank { get; set; }
    public bool IsBlacklisted { get; set; }

    [MaxLength(1000)]
    public string? BlacklistReason { get; set; }
}
