using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Action { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? EntityId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
