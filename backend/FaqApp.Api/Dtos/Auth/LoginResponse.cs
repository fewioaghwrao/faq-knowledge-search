namespace FaqApp.Api.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string Role { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}
