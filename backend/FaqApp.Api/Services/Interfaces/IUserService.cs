using FaqApp.Api.Dtos.Users;

namespace FaqApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<List<UserListItemDto>> GetUsersAsync();

    Task<bool> UpdateRoleAsync(string userId, string role);

    Task<bool> UpdateStatusAsync(string userId, bool isActive);
}
