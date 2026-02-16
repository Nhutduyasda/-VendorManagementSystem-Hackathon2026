using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.DTOs.Products;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
}

public class CreateCategoryRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    public int CurrentStock { get; set; }
    public int SupplierCount { get; set; }
}

public class CreateProductRequest
{
    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int CategoryId { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    public int CurrentStock { get; set; }
}

public class UpdateProductRequest : CreateProductRequest
{
    public int Id { get; set; }
}

public class SupplierProductDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PriceListDto
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; }
}
