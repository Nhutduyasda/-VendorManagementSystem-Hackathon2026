using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Repositories;

public class PriceListRepository : IPriceListRepository
{
    private readonly ApplicationDbContext _db;

    public PriceListRepository(ApplicationDbContext db) => _db = db;

    public async Task<List<PriceListDto>> GetPriceHistoryAsync(int supplierId, int? productId = null)
    {
        var query = _db.PriceLists
            .Where(pl => pl.SupplierId == supplierId);

        if (productId.HasValue)
            query = query.Where(pl => pl.ProductId == productId.Value);

        return await query
            .OrderByDescending(pl => pl.EffectiveDate)
            .Select(pl => new PriceListDto
            {
                Id = pl.Id,
                SupplierId = pl.SupplierId,
                ProductId = pl.ProductId,
                Price = pl.Price,
                EffectiveDate = pl.EffectiveDate
            })
            .ToListAsync();
    }

    public async Task<PriceListDto> AddPriceAsync(int supplierId, int productId, decimal price)
    {
        var entry = new PriceList
        {
            SupplierId = supplierId,
            ProductId = productId,
            Price = price,
            EffectiveDate = DateTime.UtcNow
        };

        _db.PriceLists.Add(entry);

        var sp = await _db.SupplierProducts
            .FirstOrDefaultAsync(x => x.SupplierId == supplierId && x.ProductId == productId);

        if (sp != null)
        {
            sp.Price = price;
            sp.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new PriceListDto
        {
            Id = entry.Id,
            SupplierId = entry.SupplierId,
            ProductId = entry.ProductId,
            Price = entry.Price,
            EffectiveDate = entry.EffectiveDate
        };
    }

    public async Task<List<PriceComparisonDto>> GetPriceComparisonAsync(int? productId = null)
    {
        var query = _db.SupplierProducts
            .Include(sp => sp.Product)
            .Include(sp => sp.Supplier)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(sp => sp.ProductId == productId.Value);

        var grouped = await query
            .GroupBy(sp => new { sp.ProductId, sp.Product.Name, sp.Product.SKU })
            .Select(g => new PriceComparisonDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                SKU = g.Key.SKU,
                SupplierPrices = g.Select(sp => new SupplierPriceDto
                {
                    SupplierId = sp.SupplierId,
                    SupplierName = sp.Supplier.Name,
                    Price = sp.Price,
                    EffectiveDate = sp.UpdatedAt
                }).ToList()
            })
            .ToListAsync();

        foreach (var item in grouped)
        {
            if (item.SupplierPrices.Any())
            {
                var minPrice = item.SupplierPrices.Min(sp => sp.Price);
                foreach (var sp in item.SupplierPrices)
                    sp.IsCheapest = sp.Price == minPrice;
            }
        }

        return grouped;
    }
}
