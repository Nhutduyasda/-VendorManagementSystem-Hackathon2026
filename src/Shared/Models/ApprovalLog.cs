using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.Models;

public class ApprovalLog
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    [Required]
    public string ApproverId { get; set; } = string.Empty;

    public ApprovalAction Action { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}
