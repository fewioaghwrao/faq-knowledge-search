using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FaqApp.Api.Dtos.Auth;
using FaqApp.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FaqApp.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var adminEmail = _configuration["AdminSeed:Email"];
        var adminPassword = _configuration["AdminSeed:Password"];

        if (request.Email != adminEmail || request.Password != adminPassword)
        {
            return Task.FromResult<LoginResponse?>(null);
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(
            int.Parse(_configuration["JwtSettings:ExpiresInMinutes"] ?? "60"));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Email),
            new(ClaimTypes.Email, request.Email),
            new(ClaimTypes.Role, "Admin")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult<LoginResponse?>(new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAt = expiresAt,
            Role = "Admin"
        });
    }
}