namespace SmartWMS.API.Infrastructure.Services.Email;

/// <summary>
/// Email service interface for sending emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email
    /// </summary>
    Task<EmailResult> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send an email using a template
    /// </summary>
    Task<EmailResult> SendTemplatedEmailAsync(
        string templateCode,
        Dictionary<string, string> placeholders,
        IEnumerable<string> recipients,
        string? subject = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Email message to send
/// </summary>
public class EmailMessage
{
    public required IEnumerable<string> To { get; set; }
    public IEnumerable<string>? Cc { get; set; }
    public IEnumerable<string>? Bcc { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public string? ReplyTo { get; set; }
    public Dictionary<string, byte[]>? Attachments { get; set; }

    /// <summary>
    /// Optional: Override sender email
    /// </summary>
    public string? FromEmail { get; set; }

    /// <summary>
    /// Optional: Override sender name
    /// </summary>
    public string? FromName { get; set; }
}

/// <summary>
/// Result of email sending operation
/// </summary>
public class EmailResult
{
    public bool Success { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }

    public static EmailResult Succeeded(string? messageId = null) => new()
    {
        Success = true,
        MessageId = messageId
    };

    public static EmailResult Failed(string errorMessage, Exception? exception = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        Exception = exception
    };
}
