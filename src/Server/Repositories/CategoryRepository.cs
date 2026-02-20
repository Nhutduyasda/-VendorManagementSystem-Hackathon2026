using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context) => _context = context;

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ProductCount = c.Products.Count
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ProductCount = 0
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryRequest request)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = request.Name;
        category.Description = request.Description;
        await _context.SaveChangesAsync();

        return await GetCategoryByIdAsync(id);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;
        if (category.Products.Count != 0)
            throw new InvalidOperationException("Cannot delete category with existing products.");

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }
}
