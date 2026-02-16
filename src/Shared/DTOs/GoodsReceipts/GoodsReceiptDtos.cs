using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.DTOs.GoodsReceipts;

public class GoodsReceiptDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime ReceivedDate { get; set; }
    public string ReceivedBy { get; set; } = string.Empty;
    public string ReceivedByName { get; set; } = string.Empty;
    public string? QualityNote { get; set; }
    public List<GoodsReceiptItemDto> Items { get; set; } = [];
}

public class GoodsReceiptItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int OrderedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public string? Discrepancy { get; set; }
}

public class CreateGoodsReceiptRequest
{
    [Required]
    public int PurchaseOrderId { get; set; }

    [MaxLength(2000)]
    public string? QualityNote { get; set; }

    [Required, MinLength(1)]
    public List<CreateGoodsReceiptItemRequest> Items { get; set; } = [];
}

public class CreateGoodsReceiptItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int ReceivedQuantity { get; set; }

    [MaxLength(500)]
    public string? Discrepancy { get; set; }
}
