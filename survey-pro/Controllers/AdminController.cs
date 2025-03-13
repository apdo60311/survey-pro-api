using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using survey_pro.Interfaces;
using survey_pro.Models;

namespace survey_pro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;

        public AdminController(IUserService userService, IRoleService roleService)
        {
            _userService = userService;
            _roleService = roleService;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return Ok(users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.Roles
            }));
        }

        [HttpPost("roles/create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            var result = await _roleService.CreateRoleAsync(request.RoleName);

            if (result)
                return Ok(new { message = $"Role '{request.RoleName}' created successfully" });

            return BadRequest(new { message = $"Role '{request.RoleName}' already exists" });
        }

        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRoleToUser(string userId, [FromBody] AssignRoleRequest request)
        {
            var user = await _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _roleService.AddUserToRoleAsync(userId, request.RoleName);

            if (result)
                return Ok(new { message = $"Role '{request.RoleName}' assigned to user successfully" });

            return BadRequest(new { message = "Failed to assign role or user already has this role" });
        }

        [HttpDelete("users/{userId}/roles/{role}")]
        public async Task<IActionResult> RemoveRoleFromUser(string userId, string role)
        {
            var user = await _userService.GetById(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _roleService.RemoveUserFromRoleAsync(userId, role);

            if (result)
                return Ok(new { message = $"Role '{role}' removed from user successfully" });

            return BadRequest(new { message = "Failed to remove role or user doesn't have this role" });
        }
    }

}
