using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Repositories;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Enums;

using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Server.Data;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierRepository _repo;
    private readonly IPriceListRepository _priceRepo;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly ApplicationDbContext _context;

    public SuppliersController(ISupplierRepository repo, IPriceListRepository priceRepo, ICloudinaryService cloudinaryService, ApplicationDbContext context)
    {
        _repo = repo;
        _priceRepo = priceRepo;
        _cloudinaryService = cloudinaryService;
        _context = context;
    }



    // ... (keep existing methods until UploadLogo)

    // ...





    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(int id)
    {
        var supplier = await _repo.GetByIdAsync(id);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpGet("{id:int}/overview")]
    public async Task<ActionResult<ApiResponse<SupplierOverviewDto>>> GetOverview(int id)
    {
        var overview = await _repo.GetOverviewAsync(id);
        if (overview == null)
            return NotFound(ApiResponse<SupplierOverviewDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierOverviewDto>.Ok(overview));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplier = await _repo.CreateAsync(request);
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        if (id != request.Id)
            return BadRequest(ApiResponse<SupplierDto>.Fail("ID mismatch"));

        var supplier = await _repo.UpdateAsync(id, request);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSupplier(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Supplier not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Supplier deleted"));
    }

    [HttpPost("{id:int}/blacklist")]
    [Authorize(Roles = "Admin,Manager")]    // Duplicate GetSuppliers method removed
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Blacklist(int id, [FromBody] BlacklistRequest request)
    {
        var supplier = await _repo.BlacklistAsync(id, request.Reason);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPost("{id:int}/unblacklist")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> Unblacklist(int id)
    {
        var supplier = await _repo.UnblacklistAsync(id);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPost("{id:int}/products")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<bool>>> LinkProducts(int id, [FromBody] LinkSupplierProductsRequest request)
    {
        var result = await _repo.LinkProductsAsync(id, request.Products);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Supplier not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Products linked"));
    }

    [HttpGet("{id:int}/prices")]
    public async Task<ActionResult<ApiResponse<List<PriceListDto>>>> GetPriceHistory(int id, [FromQuery] int? productId = null)
    {
        var history = await _priceRepo.GetPriceHistoryAsync(id, productId);
        return Ok(ApiResponse<List<PriceListDto>>.Ok(history));
    }

    [HttpPost("{id:int}/prices")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<PriceListDto>>> AddPrice(int id, [FromQuery] int productId, [FromQuery] decimal price)
    {
        var entry = await _priceRepo.AddPriceAsync(id, productId, price);
        return Ok(ApiResponse<PriceListDto>.Ok(entry));
    }

    [HttpGet("price-comparison")]
    public async Task<ActionResult<ApiResponse<List<PriceComparisonDto>>>> GetPriceComparison([FromQuery] int? productId = null)
    {
        var comparison = await _priceRepo.GetPriceComparisonAsync(productId);
        return Ok(ApiResponse<List<PriceComparisonDto>>.Ok(comparison));
    }
    
    [HttpPost("{id:int}/upload-logo")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<string>>> UploadLogo(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided"));

        if (file.Length > 2 * 1024 * 1024) 
            return BadRequest(ApiResponse<string>.Fail("File size exceeds 2MB limit"));
        
        if (!file.ContentType.StartsWith("image/")) 
            return BadRequest(ApiResponse<string>.Fail("File is not an image"));

        try
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound(ApiResponse<string>.Fail("Supplier not found"));

            var url = await _cloudinaryService.UploadImageAsync(file, "suppliers");
            
            supplier.LogoUrl = url;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.Ok(url, "Logo uploaded"));
        }
        catch (Exception ex)
        {
             return StatusCode(500, ApiResponse<string>.Fail($"Internal server error: {ex.Message}"));
        }
    }

    [HttpPost("import")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<List<SupplierDto>>>> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<List<SupplierDto>>.Fail("No file provided"));

        using var stream = file.OpenReadStream();
        var imported = await _repo.ImportFromExcelAsync(stream);
        return Ok(ApiResponse<List<SupplierDto>>.Ok(imported, $"{imported.Count} suppliers imported"));
    }

    [HttpGet("export")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Export([FromQuery] string? search = null, [FromQuery] SupplierStatus? status = null)
    {
        var bytes = await _repo.ExportToExcelAsync(search, status);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Suppliers.xlsx");
    }
}
