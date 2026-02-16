using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public Enums.NotificationType Type { get; set; } = Enums.NotificationType.Info;

    [Required, MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
