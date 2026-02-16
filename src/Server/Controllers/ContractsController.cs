using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Contracts;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;

    public ContractsController(IContractService contractService) => _contractService = contractService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ContractDto>>>> GetContracts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? supplierId = null)
    {
        var result = await _contractService.GetContractsAsync(page, pageSize, supplierId);
        return Ok(ApiResponse<PagedResult<ContractDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> GetContract(int id)
    {
        var contract = await _contractService.GetContractByIdAsync(id);
        if (contract == null)
            return NotFound(ApiResponse<ContractDto>.Fail("Contract not found"));

        return Ok(ApiResponse<ContractDto>.Ok(contract));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> CreateContract([FromBody] CreateContractRequest request)
    {
        var contract = await _contractService.CreateContractAsync(request);
        return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, ApiResponse<ContractDto>.Ok(contract));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<ContractDto>>> UpdateContract(int id, [FromBody] UpdateContractRequest request)
    {
        if (id != request.Id)
            return BadRequest(ApiResponse<ContractDto>.Fail("ID mismatch"));

        var contract = await _contractService.UpdateContractAsync(request);
        if (contract == null)
            return NotFound(ApiResponse<ContractDto>.Fail("Contract not found"));

        return Ok(ApiResponse<ContractDto>.Ok(contract));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteContract(int id)
    {
        var result = await _contractService.DeleteContractAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("Contract not found"));

        return Ok(ApiResponse<bool>.Ok(true, "Contract deleted"));
    }

    [HttpGet("expiring")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ContractDto>>>> GetExpiringContracts([FromQuery] int daysAhead = 30)
    {
        var contracts = await _contractService.GetExpiringContractsAsync(daysAhead);
        return Ok(ApiResponse<IEnumerable<ContractDto>>.Ok(contracts));
    }
}
