using FaqApp.Api.Dtos.Auth;
using FaqApp.Api.Entities;
using FaqApp.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

    public AuthServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "notfound@example.com",
            Password = "Password123!"
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.Null(result);

        _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(
            x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = request.Email,
            DisplayName = "非アクティブユーザー",
            IsActive = false
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.Null(result);

        _userManagerMock.Verify(
            x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@faq-app.local",
            Password = "wrong-password"
        };

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = request.Email,
            DisplayName = "管理者",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.Null(result);

        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, request.Password), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "admin@faq-app.local",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = "user-1",
            Email = request.Email,
            DisplayName = "管理者",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result!.AccessToken));
        Assert.Equal("Admin", result.Role);
        Assert.Equal("管理者", result.DisplayName);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        _userManagerMock.Verify(x => x.FindByEmailAsync(request.Email), Times.Once);
        _userManagerMock.Verify(x => x.CheckPasswordAsync(user, request.Password), Times.Once);
        _userManagerMock.Verify(x => x.GetRolesAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_UserHasNoRole_ReturnsUserRole()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@faq-app.local",
            Password = "Password123!"
        };

        var user = new ApplicationUser
        {
            Id = "user-2",
            Email = request.Email,
            DisplayName = "一般ユーザー",
            IsActive = true
        };

        _userManagerMock
            .Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(x => x.CheckPasswordAsync(user, request.Password))
            .ReturnsAsync(true);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>());

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("User", result!.Role);
        Assert.Equal("一般ユーザー", result.DisplayName);
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
    }

    private AuthService CreateService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Secret"] = "this-is-a-test-secret-key-1234567890",
                ["JwtSettings:Issuer"] = "FaqAppTestIssuer",
                ["JwtSettings:Audience"] = "FaqAppTestAudience",
                ["JwtSettings:ExpiresInMinutes"] = "60"
            })
            .Build();

        return new AuthService(_userManagerMock.Object, configuration);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);
    }
}