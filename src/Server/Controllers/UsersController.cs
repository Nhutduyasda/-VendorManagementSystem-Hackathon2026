using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Server.Repositories;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Users;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationDbContext _db;

    public UsersController(IUserRepository userRepository, ApplicationDbContext db)
    {
        _userRepository = userRepository;
        _db = db;
    }

    [HttpGet("suppliers-lookup")]
    public async Task<ActionResult<ApiResponse<List<SupplierLookupDto>>>> GetSuppliersForLookup()
    {
        var suppliers = await _db.Suppliers
            .Where(s => s.Status != SupplierStatus.Inactive && !s.IsBlacklisted)
            .OrderBy(s => s.Name)
            .Select(s => new SupplierLookupDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();

        return Ok(ApiResponse<List<SupplierLookupDto>>.Ok(suppliers));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null)
    {
        var result = await _userRepository.GetUsersAsync(page, pageSize, search, role);
        return Ok(ApiResponse<UserListResponse>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse<UserDto>.Fail("User not found"));

        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userRepository.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ApiResponse<UserDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userRepository.UpdateUserAsync(id, request);
            if (user == null)
                return NotFound(ApiResponse<UserDto>.Fail("User not found"));

            return Ok(ApiResponse<UserDto>.Ok(user));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UserDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
    {
        var result = await _userRepository.DeleteUserAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("User not found"));

        return Ok(ApiResponse<bool>.Ok(true, "User deleted successfully"));
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleUserStatus(string id)
    {
        var result = await _userRepository.ToggleUserStatusAsync(id);
        if (!result)
            return NotFound(ApiResponse<bool>.Fail("User not found"));

        return Ok(ApiResponse<bool>.Ok(true, "User status toggled successfully"));
    }
}
