using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VendorManagementSystem.Server.Data;
using VendorManagementSystem.Shared.DTOs.Users;

namespace VendorManagementSystem.Server.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;

    public UserRepository(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task<UserListResponse> GetUsersAsync(int page, int pageSize, string? searchTerm, string? roleFilter)
    {
        var query = _userManager.Users
            .Include(u => u.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(term) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Staff";

            if (!string.IsNullOrWhiteSpace(roleFilter) && !role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            userDtos.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                Role = role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                SupplierId = user.SupplierId,
                SupplierName = user.Supplier?.Name
            });
        }

        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            var allFiltered = new List<UserDto>();
            var allUsers = await _userManager.Users
                .Include(u => u.Supplier)
                .ToListAsync();

            foreach (var user in allUsers)
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    if (!user.Email!.ToLower().Contains(term) &&
                        !user.FirstName.ToLower().Contains(term) &&
                        !user.LastName.ToLower().Contains(term))
                        continue;
                }

                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? "Staff";
                if (!role.Equals(roleFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                allFiltered.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Role = role,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    SupplierId = user.SupplierId,
                    SupplierName = user.Supplier?.Name
                });
            }

            totalCount = allFiltered.Count;
            userDtos = allFiltered
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        return new UserListResponse
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.Supplier)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = roles.FirstOrDefault() ?? "Staff",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SupplierId = user.SupplierId,
            SupplierName = user.Supplier?.Name
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
    {
        if (request.Role == "Vendor" && request.SupplierId == null)
            throw new InvalidOperationException("Vendor user must be linked to a supplier");

        string? supplierName = null;
        if (request.SupplierId.HasValue)
        {
            var supplier = await _db.Suppliers.FindAsync(request.SupplierId.Value);
            if (supplier == null)
                throw new InvalidOperationException("Supplier not found");
            supplierName = supplier.Name;
        }

        var nameParts = request.FullName.Trim().Split(' ', 2);
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            SupplierId = request.SupplierId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = request.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SupplierId = user.SupplierId,
            SupplierName = supplierName
        };
    }

    public async Task<UserDto?> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _userManager.Users
            .Include(u => u.Supplier)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;

        if (request.Role == "Vendor" && request.SupplierId == null)
            throw new InvalidOperationException("Vendor user must be linked to a supplier");

        string? supplierName = null;
        if (request.Role != "Vendor")
        {
            request.SupplierId = null;
        }

        if (request.SupplierId.HasValue)
        {
            var supplier = await _db.Suppliers.FindAsync(request.SupplierId.Value);
            if (supplier == null)
                throw new InvalidOperationException("Supplier not found");
            supplierName = supplier.Name;
        }

        var nameParts = request.FullName.Trim().Split(' ', 2);
        user.FirstName = nameParts[0];
        user.LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;
        user.Email = request.Email;
        user.UserName = request.Email;
        user.IsActive = request.IsActive;
        user.SupplierId = request.SupplierId;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, request.Role);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Role = request.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            SupplierId = user.SupplierId,
            SupplierName = supplierName
        };
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
