namespace VendorManagementSystem.Shared.DTOs.Vendors;

public class VendorDashboardDto
{
    public int TotalPurchaseOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
}
