using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context) => _context = context;

    public async Task<ProductListResponse> GetProductsAsync(int page, int pageSize, string? searchTerm, int? categoryId)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.SKU.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Unit = p.Unit,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                MinStock = p.MinStock,
                MaxStock = p.MaxStock,
                CurrentStock = p.CurrentStock,
                IsActive = p.IsActive,
                SupplierCount = p.SupplierProducts.Count
            })
            .ToListAsync();

        return new ProductListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                SKU = p.SKU,
                Name = p.Name,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Unit = p.Unit,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                MinStock = p.MinStock,
                MaxStock = p.MaxStock,
                CurrentStock = p.CurrentStock,
                IsActive = p.IsActive,
                SupplierCount = p.SupplierProducts.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        var product = new Product
        {
            SKU = request.SKU,
            Name = request.Name,
            CategoryId = request.CategoryId,
            Unit = request.Unit,
            Description = request.Description,
            MinStock = request.MinStock,
            MaxStock = request.MaxStock,
            CurrentStock = request.CurrentStock,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return null;

        product.SKU = request.SKU;
        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Unit = request.Unit;
        product.Description = request.Description;
        product.MinStock = request.MinStock;
        product.MaxStock = request.MaxStock;
        product.CurrentStock = request.CurrentStock;

        await _context.SaveChangesAsync();
        return await GetProductByIdAsync(id);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ProductDto>> ImportFromExcelAsync(Stream fileStream)
    {
        var imported = new List<Product>();

        using var package = new OfficeOpenXml.ExcelPackage(fileStream);
        var worksheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("No worksheet found in the Excel file.");

        var rowCount = worksheet.Dimension?.Rows ?? 0;

        for (int row = 2; row <= rowCount; row++)
        {
            var sku = worksheet.Cells[row, 1].Text?.Trim();
            var name = worksheet.Cells[row, 2].Text?.Trim();
            var categoryName = worksheet.Cells[row, 3].Text?.Trim();
            var unit = worksheet.Cells[row, 4].Text?.Trim();
            var description = worksheet.Cells[row, 5].Text?.Trim();
            var minStockText = worksheet.Cells[row, 6].Text?.Trim();
            var maxStockText = worksheet.Cells[row, 7].Text?.Trim();

            if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(name))
                continue;

            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.SKU == sku);
            if (existingProduct != null) continue;

            int categoryId = 0;
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
                if (category == null)
                {
                    category = new Category { Name = categoryName };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                categoryId = category.Id;
            }

            int.TryParse(minStockText, out var minStock);
            int.TryParse(maxStockText, out var maxStock);

            var product = new Product
            {
                SKU = sku,
                Name = name,
                CategoryId = categoryId,
                Unit = string.IsNullOrWhiteSpace(unit) ? null : unit,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                MinStock = minStock,
                MaxStock = maxStock,
                IsActive = true
            };

            _context.Products.Add(product);
            imported.Add(product);
        }

        await _context.SaveChangesAsync();

        var result = new List<ProductDto>();
        foreach (var p in imported)
        {
            var dto = await GetProductByIdAsync(p.Id);
            if (dto != null) result.Add(dto);
        }

        return result;
    }
}
