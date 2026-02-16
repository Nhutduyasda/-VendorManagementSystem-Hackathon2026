using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Contracts;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Services;

public interface IContractService
{
    Task<PagedResult<ContractDto>> GetContractsAsync(int page = 1, int pageSize = 10, int? supplierId = null);
    Task<ContractDto?> GetContractByIdAsync(int id);
    Task<ContractDto> CreateContractAsync(CreateContractRequest request);
    Task<ContractDto?> UpdateContractAsync(UpdateContractRequest request);
    Task<bool> DeleteContractAsync(int id);
    Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int daysAhead = 30);
}

public class ContractService : IContractService
{
    private readonly ApplicationDbContext _context;

    public ContractService(ApplicationDbContext context) => _context = context;

    private static ContractDto MapToDto(Contract c) => new()
    {
        Id = c.Id,
        SupplierId = c.SupplierId,
        SupplierName = c.Supplier.Name,
        ContractNumber = c.ContractNumber,
        SignDate = c.SignDate,
        ExpiryDate = c.ExpiryDate,
        Value = c.Value,
        FilePath = c.FilePath,
        Status = c.Status,
        CreatedAt = c.CreatedAt
    };

    public async Task<PagedResult<ContractDto>> GetContractsAsync(int page = 1, int pageSize = 10, int? supplierId = null)
    {
        var query = _context.Contracts.Include(c => c.Supplier).AsQueryable();

        if (supplierId.HasValue)
            query = query.Where(c => c.SupplierId == supplierId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier.Name,
                ContractNumber = c.ContractNumber,
                SignDate = c.SignDate,
                ExpiryDate = c.ExpiryDate,
                Value = c.Value,
                FilePath = c.FilePath,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<ContractDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ContractDto?> GetContractByIdAsync(int id)
    {
        return await _context.Contracts
            .Include(c => c.Supplier)
            .Where(c => c.Id == id)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier.Name,
                ContractNumber = c.ContractNumber,
                SignDate = c.SignDate,
                ExpiryDate = c.ExpiryDate,
                Value = c.Value,
                FilePath = c.FilePath,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ContractDto> CreateContractAsync(CreateContractRequest request)
    {
        var contract = new Contract
        {
            SupplierId = request.SupplierId,
            ContractNumber = request.ContractNumber,
            SignDate = request.SignDate,
            ExpiryDate = request.ExpiryDate,
            Value = request.Value
        };

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return (await GetContractByIdAsync(contract.Id))!;
    }

    public async Task<ContractDto?> UpdateContractAsync(UpdateContractRequest request)
    {
        var contract = await _context.Contracts.FindAsync(request.Id);
        if (contract == null) return null;

        contract.SupplierId = request.SupplierId;
        contract.ContractNumber = request.ContractNumber;
        contract.SignDate = request.SignDate;
        contract.ExpiryDate = request.ExpiryDate;
        contract.Value = request.Value;
        contract.Status = request.Status;

        await _context.SaveChangesAsync();
        return await GetContractByIdAsync(contract.Id);
    }

    public async Task<bool> DeleteContractAsync(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);
        if (contract == null) return false;

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ContractDto>> GetExpiringContractsAsync(int daysAhead = 30)
    {
        var cutoff = DateTime.UtcNow.AddDays(daysAhead);

        return await _context.Contracts
            .Include(c => c.Supplier)
            .Where(c => c.Status == Shared.Enums.ContractStatus.Active && c.ExpiryDate <= cutoff)
            .OrderBy(c => c.ExpiryDate)
            .Select(c => new ContractDto
            {
                Id = c.Id,
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier.Name,
                ContractNumber = c.ContractNumber,
                SignDate = c.SignDate,
                ExpiryDate = c.ExpiryDate,
                Value = c.Value,
                FilePath = c.FilePath,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }
}
