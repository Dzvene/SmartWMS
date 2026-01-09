namespace SmartWMS.API.Infrastructure.Identity;

/// <summary>
/// JWT authentication settings loaded from configuration.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public required string SecretKey { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
