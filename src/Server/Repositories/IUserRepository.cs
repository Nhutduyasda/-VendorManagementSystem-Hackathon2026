using VendorManagementSystem.Shared.DTOs.Users;

namespace VendorManagementSystem.Server.Repositories;

public interface IUserRepository
{
    Task<UserListResponse> GetUsersAsync(int page, int pageSize, string? searchTerm, string? roleFilter);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto?> UpdateUserAsync(string id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ToggleUserStatusAsync(string id);
}
