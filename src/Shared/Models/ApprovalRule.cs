using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class ApprovalRule
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string RoleName { get; set; } = string.Empty;

    public decimal MaxAmount { get; set; }
}
