using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ApplicationDbContext _context;

    public DocumentsController(ICloudinaryService cloudinaryService, ApplicationDbContext context)
    {
        _cloudinaryService = cloudinaryService;
        _context = context;
    }

    [HttpPost("upload/{supplierId:int}")]
    [Authorize(Roles = "Admin,Manager,Vendor")]
    public async Task<ActionResult<ApiResponse<Document>>> Upload(int supplierId, IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(ApiResponse<Document>.Fail("File is empty"));

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            return BadRequest(ApiResponse<Document>.Fail("File size exceeds 10 MB limit"));

        var supplier = await _context.Suppliers.FindAsync(supplierId);
        if (supplier == null)
            return NotFound(ApiResponse<Document>.Fail("Supplier not found"));

        using var stream = file.OpenReadStream();
        var (url, publicId) = await _cloudinaryService.UploadFileAsync(stream, file.FileName);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var document = new Document
        {
            FileName = file.FileName,
            Url = url,
            ContentType = file.ContentType,
            FileSize = file.Length,
            PublicId = publicId,
            SupplierId = supplierId,
            UploadedByUserId = userId
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<Document>.Ok(document, "File uploaded successfully"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return NotFound(ApiResponse<bool>.Fail("Document not found"));

        if (!string.IsNullOrEmpty(document.PublicId))
            await _cloudinaryService.DeleteFileAsync(document.PublicId);

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();

        return Ok(ApiResponse<bool>.Ok(true, "Document deleted"));
    }
}
