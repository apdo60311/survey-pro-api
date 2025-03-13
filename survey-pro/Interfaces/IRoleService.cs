using System;

namespace survey_pro.Interfaces;

public interface IRoleService
{
    Task<bool> AddUserToRoleAsync(string userId, string role);
    Task<bool> RemoveUserFromRoleAsync(string userId, string role);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<List<string>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string role);
    Task<bool> CreateRoleAsync(string role);
}
