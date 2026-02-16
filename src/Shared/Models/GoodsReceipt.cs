using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class GoodsReceipt
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public string ReceivedBy { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? QualityNote { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public ICollection<GoodsReceiptItem> Items { get; set; } = [];
}
