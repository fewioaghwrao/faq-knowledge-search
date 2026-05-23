using FaqApp.Api.Controllers;
using FaqApp.Api.Dtos.Auth;
using FaqApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();

    private AuthController CreateController()
    {
        return new AuthController(_authServiceMock.Object);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@faq-app.local",
            Password = "Password123!"
        };

        var response = new LoginResponse
        {
            AccessToken = "dummy-jwt-token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            DisplayName = "管理者",
            Role = "Admin"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync(response);

        var controller = CreateController();

        // Act
        var result = await controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);

        Assert.Same(response, okResult.Value);

        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "wrong@example.com",
            Password = "wrong-password"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request))
            .ReturnsAsync((LoginResponse?)null);

        var controller = CreateController();

        // Act
        var result = await controller.Login(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
        Assert.IsType<UnauthorizedObjectResult>(actionResult.Result);

        _authServiceMock.Verify(x => x.LoginAsync(request), Times.Once);
    }
}
