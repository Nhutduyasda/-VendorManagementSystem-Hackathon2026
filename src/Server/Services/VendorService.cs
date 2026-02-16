using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Services;

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request);
    Task<SupplierDto?> UpdateSupplierAsync(UpdateSupplierRequest request);
    Task<bool> DeleteSupplierAsync(int id);
}

public class SupplierService : ISupplierService
{
    private readonly ApplicationDbContext _context;

    public SupplierService(ApplicationDbContext context) => _context = context;

    public async Task<PagedResult<SupplierDto>> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _context.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Email.ToLower().Contains(term) ||
                (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                TaxCode = s.TaxCode,
                Address = s.Address,
                Phone = s.Phone,
                Email = s.Email,
                ContactPerson = s.ContactPerson,
                LogoUrl = s.LogoUrl,
                Status = s.Status,
                Rank = s.Rank,
                IsBlacklisted = s.IsBlacklisted,
                BlacklistReason = s.BlacklistReason,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                ContractCount = s.Contracts.Count,
                ProductCount = s.SupplierProducts.Count,
                AverageRating = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.OverallRating) : null
            })
            .ToListAsync();

        return new PagedResult<SupplierDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        return await _context.Suppliers
            .Where(s => s.Id == id)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                TaxCode = s.TaxCode,
                Address = s.Address,
                Phone = s.Phone,
                Email = s.Email,
                ContactPerson = s.ContactPerson,
                LogoUrl = s.LogoUrl,
                Status = s.Status,
                Rank = s.Rank,
                IsBlacklisted = s.IsBlacklisted,
                BlacklistReason = s.BlacklistReason,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                ContractCount = s.Contracts.Count,
                ProductCount = s.SupplierProducts.Count,
                AverageRating = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.OverallRating) : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            TaxCode = request.TaxCode,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            ContactPerson = request.ContactPerson
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return (await GetSupplierByIdAsync(supplier.Id))!;
    }

    public async Task<SupplierDto?> UpdateSupplierAsync(UpdateSupplierRequest request)
    {
        var supplier = await _context.Suppliers.FindAsync(request.Id);
        if (supplier == null) return null;

        supplier.Name = request.Name;
        supplier.TaxCode = request.TaxCode;
        supplier.Address = request.Address;
        supplier.Phone = request.Phone;
        supplier.Email = request.Email;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Status = request.Status;
        supplier.Rank = request.Rank;
        supplier.IsBlacklisted = request.IsBlacklisted;
        supplier.BlacklistReason = request.BlacklistReason;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetSupplierByIdAsync(supplier.Id);
    }

    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return false;

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        return true;
    }
}
