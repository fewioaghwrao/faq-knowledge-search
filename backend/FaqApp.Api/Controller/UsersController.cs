using FaqApp.Api.Dtos.Users;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FaqApp.Api.Controller;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListItemDto>>> GetUsers()
    {
        var users = await _userService.GetUsersAsync();

        return Ok(users);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(
        string id,
        [FromBody] UpdateUserRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return BadRequest("Role is required.");
        }

        var result = await _userService.UpdateRoleAsync(id, request.Role);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        string id,
        [FromBody] UpdateUserStatusRequest request)
    {
        var result = await _userService.UpdateStatusAsync(id, request.IsActive);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}