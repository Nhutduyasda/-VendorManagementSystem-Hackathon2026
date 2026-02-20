using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.Models;

namespace VendorManagementSystem.Server.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICloudinaryService _cloudinaryService;

    public SuppliersController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
    {
        _context = context;
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("{id}/upload-logo")]
    public async Task<ActionResult<string>> UploadSupplierLogo(int id, IFormFile file)
    {
        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound("Supplier not found");

            if (file == null || file.Length == 0) return BadRequest("File is empty");
            if (file.Length > 2 * 1024 * 1024) return BadRequest("File size exceeds 2MB limit");
            if (!file.ContentType.StartsWith("image/")) return BadRequest("File is not an image");

            var imageUrl = await _cloudinaryService.UploadImageAsync(file, "suppliers");
            
            supplier.LogoUrl = imageUrl;
            await _context.SaveChangesAsync();

            return Ok(imageUrl);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
