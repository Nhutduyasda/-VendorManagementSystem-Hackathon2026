using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Contract
{
    public int Id { get; set; }

    public int SupplierId { get; set; }

    [Required, MaxLength(100)]
    public string ContractNumber { get; set; } = string.Empty;

    public DateTime SignDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public decimal Value { get; set; }

    [MaxLength(500)]
    public string? FilePath { get; set; }

    public Enums.ContractStatus Status { get; set; } = Enums.ContractStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Supplier Supplier { get; set; } = null!;
}
