using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.PurchaseOrders;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Models;
using System.Security.Claims;

namespace VendorManagementSystem.Server.Controllers;

[Authorize(Roles = "Vendor")]
[ApiController]
[Route("api/vendor")]
public class VendorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VendorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<int> GetCurrentUserSupplierIdAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) throw new UnauthorizedAccessException("User not found.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.SupplierId.HasValue)
        {
            throw new InvalidOperationException("User is not linked to any supplier.");
        }

        return user.SupplierId.Value;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<VendorDashboardDto>>> GetDashboard()
    {
        try
        {
            var supplierId = await GetCurrentUserSupplierIdAsync();

            var poStats = await _context.PurchaseOrders
                .Where(po => po.SupplierId == supplierId)
                .GroupBy(po => po.SupplierId)
                .Select(g => new
                {
                    TotalPOs = g.Count(),
                    TotalRevenue = g.Sum(po => po.TotalAmount)
                })
                .FirstOrDefaultAsync();

            var avgRating = await _context.VendorRatings
                .Where(vr => vr.SupplierId == supplierId)
                .AverageAsync(vr => (double?)vr.OverallRating) ?? 0;

            var dashboard = new VendorDashboardDto
            {
                TotalPurchaseOrders = poStats?.TotalPOs ?? 0,
                TotalRevenue = poStats?.TotalRevenue ?? 0,
                AverageRating = avgRating
            };

            return Ok(new ApiResponse<VendorDashboardDto>
            {
                Success = true,
                Data = dashboard
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<VendorDashboardDto> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<VendorDashboardDto> { Success = false, Message = "An error occurred fetching dashboard data." });
        }
    }

    [HttpGet("purchase-orders")]
    public async Task<ActionResult<ApiResponse<List<PurchaseOrderDto>>>> GetPurchaseOrders()
    {
        try
        {
            var supplierId = await GetCurrentUserSupplierIdAsync();

            var orders = await _context.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.Supplier)
                .Where(po => po.SupplierId == supplierId)
                .OrderByDescending(po => po.CreatedAt)
                .Select(po => new PurchaseOrderDto
                {
                    Id = po.Id,
                    PONumber = po.PONumber,
                    SupplierName = po.Supplier.Name,
                    Status = po.Status, // Enum assignment
                    TotalAmount = po.TotalAmount,
                    CreatedAt = po.CreatedAt,
                    Items = po.Items.Select(i => new PurchaseOrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        ProductSKU = i.Product.SKU,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<PurchaseOrderDto>>
            {
                Success = true,
                Data = orders
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<List<PurchaseOrderDto>> { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("my-supplier-info")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetMySupplierInfo()
    {
        try
        {
            var supplierId = await GetCurrentUserSupplierIdAsync();

            var supplier = await _context.Suppliers
                .Where(s => s.Id == supplierId)
                .Select(s => new SupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    TaxCode = s.TaxCode,
                    Address = s.Address,
                    Phone = s.Phone,
                    Email = s.Email,
                    ContactPerson = s.ContactPerson,
                    Status = s.Status,
                    Rank = s.Rank
                })
                .FirstOrDefaultAsync();

            if (supplier == null) return NotFound(new ApiResponse<SupplierDto> { Success = false, Message = "Supplier not found." });

            return Ok(new ApiResponse<SupplierDto>
            {
                Success = true,
                Data = supplier
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<SupplierDto> { Success = false, Message = ex.Message });
        }
    }
}


