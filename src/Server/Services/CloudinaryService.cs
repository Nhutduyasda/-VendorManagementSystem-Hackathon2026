using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace VendorManagementSystem.Server.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Cloudinary");
        var account = new Account(
            section["CloudName"],
            section["ApiKey"],
            section["ApiSecret"]
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file, string folder)
    {
        try
        {
            if (file.Length == 0) return string.Empty;

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                Transformation = new Transformation().Quality("auto").FetchFormat("auto")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return uploadResult.SecureUrl.ToString();
        }
        catch (Exception ex)
        {
            // Log exception here
            throw new Exception($"Image upload failed: {ex.Message}");
        }
    }

    public async Task<(string Url, string PublicId)> UploadFileAsync(Stream stream, string fileName, string folder = "documents")
    {
         var uploadParams = new RawUploadParams
         {
             File = new FileDescription(fileName, stream),
             Folder = folder
         };

         var uploadResult = await _cloudinary.UploadAsync(uploadParams);

         if (uploadResult.Error != null)
             throw new Exception(uploadResult.Error.Message);

         return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
    }

    public async Task<bool> DeleteImageAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        if (result.Error != null) return false;

        return result.Result == "ok";
    }

    public async Task<bool> DeleteFileAsync(string publicId)
    {
        var deletionParams = new DeletionParams(publicId) { ResourceType = ResourceType.Raw };
        var result = await _cloudinary.DestroyAsync(deletionParams);
        return result.Result == "ok";
    }
}
