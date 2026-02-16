using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.DTOs.PurchaseOrders;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string CreatorId { get; set; } = string.Empty;
    public string CreatorName { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = [];
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreatePurchaseOrderRequest
{
    [Required]
    public int SupplierId { get; set; }

    [MaxLength(2000)]
    public string? Note { get; set; }

    [Required, MinLength(1)]
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = [];
}

public class CreatePurchaseOrderItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}

public class ApprovalLogDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public string ApproverId { get; set; } = string.Empty;
    public string ApproverName { get; set; } = string.Empty;
    public ApprovalAction Action { get; set; }
    public string? Reason { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ApproveRejectRequest
{
    [Required]
    public ApprovalAction Action { get; set; }

    [MaxLength(1000)]
    public string? Reason { get; set; }
}
