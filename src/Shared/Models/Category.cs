using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation
    public ICollection<Product> Products { get; set; } = [];
}
