using FaqApp.Api.Controller;
using FaqApp.Api.Dtos.Users;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Controllers;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();

    private UsersController CreateController()
    {
        return new UsersController(_userServiceMock.Object);
    }

    [Fact]
    public async Task GetUsers_ReturnsOk()
    {
        // Arrange
        var users = new List<UserListItemDto>
        {
            new UserListItemDto
            {
                Id = "user-1",
                Email = "admin@faq-app.local",
                DisplayName = "管理者",
                Role = "Admin",
                IsActive = true
            }
        };

        _userServiceMock
            .Setup(x => x.GetUsersAsync())
            .ReturnsAsync(users);

        var controller = CreateController();

        // Act
        var result = await controller.GetUsers();

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<UserListItemDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.Same(users, okResult.Value);

        _userServiceMock.Verify(x => x.GetUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var userId = "user-1";
        var request = new UpdateUserRoleRequest
        {
            Role = "Admin"
        };

        _userServiceMock
            .Setup(x => x.UpdateRoleAsync(userId, request.Role))
            .ReturnsAsync(true);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateRole(userId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _userServiceMock.Verify(
            x => x.UpdateRoleAsync(userId, request.Role),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task UpdateRole_EmptyRole_ReturnsBadRequest(string role)
    {
        // Arrange
        var userId = "user-1";
        var request = new UpdateUserRoleRequest
        {
            Role = role
        };

        var controller = CreateController();

        // Act
        var result = await controller.UpdateRole(userId, request);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role is required.", badRequest.Value);

        _userServiceMock.Verify(
            x => x.UpdateRoleAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRole_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "not-found-user";
        var request = new UpdateUserRoleRequest
        {
            Role = "User"
        };

        _userServiceMock
            .Setup(x => x.UpdateRoleAsync(userId, request.Role))
            .ReturnsAsync(false);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateRole(userId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _userServiceMock.Verify(
            x => x.UpdateRoleAsync(userId, request.Role),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var userId = "user-1";
        var request = new UpdateUserStatusRequest
        {
            IsActive = false
        };

        _userServiceMock
            .Setup(x => x.UpdateStatusAsync(userId, request.IsActive))
            .ReturnsAsync(true);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateStatus(userId, request);

        // Assert
        Assert.IsType<NoContentResult>(result);

        _userServiceMock.Verify(
            x => x.UpdateStatusAsync(userId, request.IsActive),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "not-found-user";
        var request = new UpdateUserStatusRequest
        {
            IsActive = true
        };

        _userServiceMock
            .Setup(x => x.UpdateStatusAsync(userId, request.IsActive))
            .ReturnsAsync(false);

        var controller = CreateController();

        // Act
        var result = await controller.UpdateStatus(userId, request);

        // Assert
        Assert.IsType<NotFoundResult>(result);

        _userServiceMock.Verify(
            x => x.UpdateStatusAsync(userId, request.IsActive),
            Times.Once);
    }

    [Fact]
    public void UsersController_HasAuthorizeAdminAttribute()
    {
        // Act
        var attribute = typeof(UsersController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .OfType<AuthorizeAttribute>()
            .FirstOrDefault();

        // Assert
        Assert.NotNull(attribute);
        Assert.Equal("Admin", attribute!.Roles);
    }
}