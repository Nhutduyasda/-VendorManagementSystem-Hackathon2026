using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace VendorManagementSystem.Server.Services;

public interface ICloudinaryService
{
    Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string folder = "vendor-docs");
    Task<bool> DeleteFileAsync(string publicId);
}

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;

    public CloudinaryService(Cloudinary cloudinary, ILogger<CloudinaryService> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream fileStream, string fileName, string folder = "vendor-docs")
    {
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload error: {Error}", result.Error.Message);
            throw new InvalidOperationException($"File upload failed: {result.Error.Message}");
        }

        return (result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Raw
        };

        var result = await _cloudinary.DestroyAsync(deleteParams);
        return result.Result == "ok";
    }
}
