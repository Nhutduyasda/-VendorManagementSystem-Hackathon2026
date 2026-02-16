using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class PurchaseOrder
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string PONumber { get; set; } = string.Empty;

    public int SupplierId { get; set; }

    [Required]
    public string CreatorId { get; set; } = string.Empty;

    public Enums.PurchaseOrderStatus Status { get; set; } = Enums.PurchaseOrderStatus.Draft;

    public decimal TotalAmount { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public DateTime? SentAt { get; set; }

    // Navigation
    public Supplier Supplier { get; set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
    public ICollection<ApprovalLog> ApprovalLogs { get; set; } = [];
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = [];
    public AccountsPayable? AccountsPayable { get; set; }
    public VendorRating? VendorRating { get; set; }
}
