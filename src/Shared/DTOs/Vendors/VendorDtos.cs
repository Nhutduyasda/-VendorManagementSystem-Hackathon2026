using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.DTOs.Vendors;

// ── Supplier DTOs ───────────────────────────────────────────────────
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
