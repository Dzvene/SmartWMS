namespace SmartWMS.API.Infrastructure.Services.Email;

/// <summary>
/// SMTP configuration settings for email service
/// </summary>
public class SmtpSettings
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string FromEmail { get; set; } = "noreply@smartwms.local";
    public string FromName { get; set; } = "SmartWMS";
    public int TimeoutMs { get; set; } = 30000;

    /// <summary>
    /// If true, emails will be logged but not actually sent (for development)
    /// </summary>
    public bool UseMockSender { get; set; } = false;
}
