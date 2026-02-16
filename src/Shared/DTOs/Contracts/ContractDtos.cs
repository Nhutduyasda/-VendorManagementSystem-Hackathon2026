using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.DTOs.Contracts;

public class ContractDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime SignDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public decimal Value { get; set; }
    public string? FilePath { get; set; }
    public ContractStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateContractRequest
{
    [Required]
    public int SupplierId { get; set; }

    [Required, MaxLength(50)]
    public string ContractNumber { get; set; } = string.Empty;

    [Required]
    public DateTime SignDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Value { get; set; }
}

public class UpdateContractRequest : CreateContractRequest
{
    public int Id { get; set; }
    public ContractStatus Status { get; set; }
}
