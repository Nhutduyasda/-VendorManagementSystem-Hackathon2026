using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class RatingCriteria
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public double Weight { get; set; }
}
