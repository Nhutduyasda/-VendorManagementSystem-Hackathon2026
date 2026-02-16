using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Shared.Models;

public class AccountsPayable
{
    public int Id { get; set; }

    public int PurchaseOrderId { get; set; }

    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    public decimal PaidAmount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
}
