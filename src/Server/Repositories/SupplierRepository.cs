using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Enums;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _db;
    private readonly ICloudinaryService _cloudinary;

    public SupplierRepository(ApplicationDbContext db, ICloudinaryService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    public async Task<SupplierListResponse> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null, SupplierStatus? status = null, SupplierRank? rank = null, bool? isBlacklisted = null)
    {
        var query = _db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term)
                || s.Email.ToLower().Contains(term)
                || (s.TaxCode != null && s.TaxCode.ToLower().Contains(term))
                || (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(term)));
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        if (rank.HasValue)
            query = query.Where(s => s.Rank == rank.Value);

        if (isBlacklisted.HasValue)
            query = query.Where(s => s.IsBlacklisted == isBlacklisted.Value);

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
                AverageRating = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.OverallRating) : null,
                TotalPurchaseAmount = s.PurchaseOrders.Sum(po => po.TotalAmount)
            })
            .ToListAsync();

        return new SupplierListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        return await _db.Suppliers
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
                AverageRating = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.OverallRating) : null,
                TotalPurchaseAmount = s.PurchaseOrders.Sum(po => po.TotalAmount)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SupplierOverviewDto?> GetOverviewAsync(int id)
    {
        var supplier = await GetByIdAsync(id);
        if (supplier == null) return null;

        var products = await _db.SupplierProducts
            .Where(sp => sp.SupplierId == id)
            .Include(sp => sp.Product)
            .Select(sp => new Shared.DTOs.Products.SupplierProductDto
            {
                SupplierId = sp.SupplierId,
                ProductId = sp.ProductId,
                ProductName = sp.Product.Name,
                Price = sp.Price,
                UpdatedAt = sp.UpdatedAt
            })
            .ToListAsync();

        var priceHistory = await _db.PriceLists
            .Where(pl => pl.SupplierId == id)
            .OrderByDescending(pl => pl.EffectiveDate)
            .Take(50)
            .Select(pl => new Shared.DTOs.Products.PriceListDto
            {
                Id = pl.Id,
                SupplierId = pl.SupplierId,
                ProductId = pl.ProductId,
                Price = pl.Price,
                EffectiveDate = pl.EffectiveDate
            })
            .ToListAsync();

        var ratings = await _db.VendorRatings
            .Where(r => r.SupplierId == id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new SupplierRatingDto
            {
                Id = r.Id,
                PurchaseOrderId = r.PurchaseOrderId,
                RaterId = r.RaterId,
                OverallRating = r.OverallRating,
                QualityScore = r.QualityScore,
                PriceScore = r.PriceScore,
                DeliveryScore = r.DeliveryScore,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var stats = await _db.Suppliers
            .Where(s => s.Id == id)
            .Select(s => new SupplierStatsDto
            {
                TotalOrders = s.PurchaseOrders.Count,
                TotalSpent = s.PurchaseOrders.Sum(po => po.TotalAmount),
                ActiveContracts = s.Contracts.Count(c => c.Status == ContractStatus.Active),
                AverageDeliveryScore = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.DeliveryScore) : 0,
                AverageQualityScore = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.QualityScore) : 0,
                AveragePriceScore = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.PriceScore) : 0
            })
            .FirstOrDefaultAsync() ?? new SupplierStatsDto();

        return new SupplierOverviewDto
        {
            Supplier = supplier,
            Products = products,
            PriceHistory = priceHistory,
            Ratings = ratings,
            Stats = stats
        };
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request)
    {
        var supplier = new Supplier
        {
            Name = request.Name,
            TaxCode = request.TaxCode,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            ContactPerson = request.ContactPerson,
            Status = SupplierStatus.Pending,
            Rank = SupplierRank.Unranked,
            CreatedAt = DateTime.UtcNow
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(supplier.Id))!;
    }

    public async Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
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

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return false;

        _db.Suppliers.Remove(supplier);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SupplierDto?> BlacklistAsync(int id, string reason)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return null;

        supplier.IsBlacklisted = true;
        supplier.BlacklistReason = reason;
        supplier.Status = SupplierStatus.Suspended;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<SupplierDto?> UnblacklistAsync(int id)
    {
        var supplier = await _db.Suppliers.FindAsync(id);
        if (supplier == null) return null;

        supplier.IsBlacklisted = false;
        supplier.BlacklistReason = null;
        supplier.Status = SupplierStatus.Active;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> LinkProductsAsync(int supplierId, List<SupplierProductItemDto> products)
    {
        var supplier = await _db.Suppliers.FindAsync(supplierId);
        if (supplier == null) return false;

        var existing = await _db.SupplierProducts
            .Where(sp => sp.SupplierId == supplierId)
            .ToListAsync();
        _db.SupplierProducts.RemoveRange(existing);

        var now = DateTime.UtcNow;
        var newLinks = products.Select(p => new SupplierProduct
        {
            SupplierId = supplierId,
            ProductId = p.ProductId,
            Price = p.Price,
            UpdatedAt = now
        });

        _db.SupplierProducts.AddRange(newLinks);

        foreach (var p in products)
        {
            _db.PriceLists.Add(new PriceList
            {
                SupplierId = supplierId,
                ProductId = p.ProductId,
                Price = p.Price,
                EffectiveDate = now
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<SupplierDto>> ImportFromExcelAsync(Stream stream)
    {
        var created = new List<SupplierDto>();
        using var package = new ExcelPackage(stream);
        var ws = package.Workbook.Worksheets.FirstOrDefault();
        if (ws == null) return created;

        for (int row = 2; row <= ws.Dimension.End.Row; row++)
        {
            var name = ws.Cells[row, 1].Text?.Trim();
            var email = ws.Cells[row, 2].Text?.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email)) continue;

            var exists = await _db.Suppliers.AnyAsync(s => s.Email == email);
            if (exists) continue;

            var supplier = new Supplier
            {
                Name = name,
                Email = email,
                TaxCode = ws.Cells[row, 3].Text?.Trim(),
                Address = ws.Cells[row, 4].Text?.Trim(),
                Phone = ws.Cells[row, 5].Text?.Trim(),
                ContactPerson = ws.Cells[row, 6].Text?.Trim(),
                Status = SupplierStatus.Pending,
                Rank = SupplierRank.Unranked,
                CreatedAt = DateTime.UtcNow
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            var dto = await GetByIdAsync(supplier.Id);
            if (dto != null) created.Add(dto);
        }

        return created;
    }

    public async Task<byte[]> ExportToExcelAsync(string? search = null, SupplierStatus? status = null)
    {
        var query = _db.Suppliers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || s.Email.ToLower().Contains(term));
        }

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var suppliers = await query
            .OrderBy(s => s.Name)
            .Include(s => s.SupplierProducts)
            .Include(s => s.Contracts)
            .Include(s => s.VendorRatings)
            .Include(s => s.PurchaseOrders)
            .ToListAsync();

        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Suppliers");

        ws.Cells[1, 1].Value = "Name";
        ws.Cells[1, 2].Value = "Email";
        ws.Cells[1, 3].Value = "Tax Code";
        ws.Cells[1, 4].Value = "Address";
        ws.Cells[1, 5].Value = "Phone";
        ws.Cells[1, 6].Value = "Contact Person";
        ws.Cells[1, 7].Value = "Status";
        ws.Cells[1, 8].Value = "Rank";
        ws.Cells[1, 9].Value = "Blacklisted";
        ws.Cells[1, 10].Value = "Products";
        ws.Cells[1, 11].Value = "Contracts";
        ws.Cells[1, 12].Value = "Avg Rating";
        ws.Cells[1, 13].Value = "Total Purchase";

        using (var range = ws.Cells[1, 1, 1, 13])
        {
            range.Style.Font.Bold = true;
        }

        for (int i = 0; i < suppliers.Count; i++)
        {
            var s = suppliers[i];
            var row = i + 2;
            ws.Cells[row, 1].Value = s.Name;
            ws.Cells[row, 2].Value = s.Email;
            ws.Cells[row, 3].Value = s.TaxCode;
            ws.Cells[row, 4].Value = s.Address;
            ws.Cells[row, 5].Value = s.Phone;
            ws.Cells[row, 6].Value = s.ContactPerson;
            ws.Cells[row, 7].Value = s.Status.ToString();
            ws.Cells[row, 8].Value = s.Rank.ToString();
            ws.Cells[row, 9].Value = s.IsBlacklisted ? "Yes" : "No";
            ws.Cells[row, 10].Value = s.SupplierProducts.Count;
            ws.Cells[row, 11].Value = s.Contracts.Count;
            ws.Cells[row, 12].Value = s.VendorRatings.Any() ? s.VendorRatings.Average(r => r.OverallRating) : 0;
            ws.Cells[row, 13].Value = s.PurchaseOrders.Sum(po => po.TotalAmount);
        }

        ws.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<string?> UploadLogoAsync(int supplierId, Stream stream, string fileName, string contentType)
    {
        var supplier = await _db.Suppliers.FindAsync(supplierId);
        if (supplier == null) return null;

        var (imageUrl, _) = await _cloudinary.UploadFileAsync(stream, fileName, "suppliers");
        if (string.IsNullOrEmpty(imageUrl)) return null;

        supplier.LogoUrl = imageUrl;
        supplier.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return imageUrl;
    }
}
