using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Vendors;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService) => _supplierService = supplierService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SupplierDto>>>> GetSuppliers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _supplierService.GetSuppliersAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResult<SupplierDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> GetSupplier(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        var supplier = await _supplierService.CreateSupplierAsync(request);
        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.Id }, ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<SupplierDto>>> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        if (id != request.Id)
            return BadRequest(ApiResponse<SupplierDto>.Fail("ID mismatch"));

        var supplier = await _supplierService.UpdateSupplierAsync(request);
        if (supplier == null)
            return NotFound(ApiResponse<SupplierDto>.Fail("Supplier not found"));

        return Ok(ApiResponse<SupplierDto>.Ok(supplier));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSupplier(int id)
    {
        var result = await _supplierService.DeleteSupplierAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Supplier not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Supplier deleted"));
    }
}
