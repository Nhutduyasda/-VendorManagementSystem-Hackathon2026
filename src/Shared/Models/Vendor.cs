using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? TaxCode { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [EmailAddress, MaxLength(256)]
    public string? Email { get; set; }

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    public Enums.SupplierStatus Status { get; set; } = Enums.SupplierStatus.Pending;

    public Enums.SupplierRank Rank { get; set; } = Enums.SupplierRank.Unranked;

    public bool IsBlacklisted { get; set; }

    [MaxLength(1000)]
    public string? BlacklistReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<SupplierProduct> SupplierProducts { get; set; } = [];
    public ICollection<PriceList> PriceLists { get; set; } = [];
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
    public ICollection<VendorRating> VendorRatings { get; set; } = [];
    public ICollection<Contract> Contracts { get; set; } = [];
    public ICollection<Document> Documents { get; set; } = [];
}
