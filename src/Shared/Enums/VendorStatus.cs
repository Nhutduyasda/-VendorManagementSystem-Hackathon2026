namespace VendorManagementSystem.Shared.Enums;

public enum SupplierStatus
{
    Pending = 0,
    Approved = 1,
    Active = 2,
    Suspended = 3,
    Inactive = 4
}

public enum SupplierRank
{
    Unranked = 0,
    Bronze = 1,
    Silver = 2,
    Gold = 3,
    Platinum = 4
}

public enum PurchaseOrderStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Sent = 4,
    PartiallyReceived = 5,
    Received = 6,
    Completed = 7,
    Cancelled = 8
}

public enum ApprovalAction
{
    Approved = 0,
    Rejected = 1,
    ReturnedForRevision = 2
}

public enum PaymentStatus
{
    Unpaid = 0,
    PartiallyPaid = 1,
    Paid = 2,
    Overdue = 3
}
