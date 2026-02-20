using Microsoft.AspNetCore.Http;

namespace VendorManagementSystem.Server.Services;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file, string folder);
    Task<(string Url, string PublicId)> UploadFileAsync(Stream stream, string fileName, string folder = "documents");
    Task<bool> DeleteImageAsync(string publicId);
    Task<bool> DeleteFileAsync(string publicId);
}
