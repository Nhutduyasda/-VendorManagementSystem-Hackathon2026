using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Data;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    [MaxLength(100)]
    public string? Department { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public int? SupplierId { get; set; }
    public virtual Supplier? Supplier { get; set; }

    public ICollection<Notification> Notifications { get; set; } = [];
}
