using System.ComponentModel.DataAnnotations;

namespace VendorManagementSystem.Shared.Models;

public class Document
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Url { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    [MaxLength(300)]
    public string? PublicId { get; set; }

    public int SupplierId { get; set; }

    public string? UploadedByUserId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Supplier Supplier { get; set; } = null!;
}
