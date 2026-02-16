using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class GoodsReceiptItem
{
    public int Id { get; set; }

    public int GoodsReceiptId { get; set; }

    public int ProductId { get; set; }

    public int OrderedQuantity { get; set; }

    public int ReceivedQuantity { get; set; }

    [MaxLength(500)]
    public string? Discrepancy { get; set; }

    // Navigation
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
