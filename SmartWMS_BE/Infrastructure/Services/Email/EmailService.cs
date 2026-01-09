using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmartWMS.API.Infrastructure.Services.Email;

/// <summary>
/// SMTP-based email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SmtpSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<EmailResult> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (!message.To.Any())
        {
            return EmailResult.Failed("No recipients specified");
        }

        // Mock mode for development
        if (_settings.UseMockSender)
        {
            _logger.LogInformation(
                "[MOCK EMAIL] To: {Recipients}, Subject: {Subject}, Body length: {BodyLength}",
                string.Join(", ", message.To),
                message.Subject,
                message.Body.Length);

            return EmailResult.Succeeded($"mock-{Guid.NewGuid():N}");
        }

        try
        {
            using var smtpClient = CreateSmtpClient();
            using var mailMessage = CreateMailMessage(message);

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Recipients}, Subject: {Subject}",
                string.Join(", ", message.To),
                message.Subject);

            return EmailResult.Succeeded(mailMessage.Headers["Message-Id"]);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "SMTP error sending email to {Recipients}: {Error}",
                string.Join(", ", message.To), ex.Message);
            return EmailResult.Failed($"SMTP error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}: {Error}",
                string.Join(", ", message.To), ex.Message);
            return EmailResult.Failed($"Failed to send email: {ex.Message}", ex);
        }
    }

    public async Task<EmailResult> SendTemplatedEmailAsync(
        string templateCode,
        Dictionary<string, string> placeholders,
        IEnumerable<string> recipients,
        string? subject = null,
        CancellationToken cancellationToken = default)
    {
        // Get template from database or file
        var template = GetEmailTemplate(templateCode);

        if (template == null)
        {
            _logger.LogWarning("Email template not found: {TemplateCode}", templateCode);
            return EmailResult.Failed($"Email template '{templateCode}' not found");
        }

        // Replace placeholders
        var body = ReplacePlaceholders(template.Body, placeholders);
        var finalSubject = ReplacePlaceholders(subject ?? template.Subject, placeholders);

        var message = new EmailMessage
        {
            To = recipients,
            Subject = finalSubject,
            Body = body,
            IsHtml = template.IsHtml
        };

        return await SendEmailAsync(message, cancellationToken);
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Timeout = _settings.TimeoutMs,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrEmpty(_settings.Username))
        {
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        return client;
    }

    private MailMessage CreateMailMessage(EmailMessage message)
    {
        var fromEmail = message.FromEmail ?? _settings.FromEmail;
        var fromName = message.FromName ?? _settings.FromName;

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml,
            Priority = MailPriority.Normal
        };

        // Add recipients
        foreach (var to in message.To)
        {
            if (IsValidEmail(to))
                mailMessage.To.Add(to);
        }

        if (message.Cc != null)
        {
            foreach (var cc in message.Cc)
            {
                if (IsValidEmail(cc))
                    mailMessage.CC.Add(cc);
            }
        }

        if (message.Bcc != null)
        {
            foreach (var bcc in message.Bcc)
            {
                if (IsValidEmail(bcc))
                    mailMessage.Bcc.Add(bcc);
            }
        }

        if (!string.IsNullOrEmpty(message.ReplyTo) && IsValidEmail(message.ReplyTo))
        {
            mailMessage.ReplyToList.Add(message.ReplyTo);
        }

        // Add attachments
        if (message.Attachments != null)
        {
            foreach (var attachment in message.Attachments)
            {
                var stream = new MemoryStream(attachment.Value);
                mailMessage.Attachments.Add(new Attachment(stream, attachment.Key));
            }
        }

        return mailMessage;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private string ReplacePlaceholders(string template, Dictionary<string, string> placeholders)
    {
        if (string.IsNullOrEmpty(template) || placeholders == null)
            return template;

        foreach (var kvp in placeholders)
        {
            template = template.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            template = template.Replace($"${{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
            template = template.Replace($"{{{kvp.Key}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private EmailTemplate? GetEmailTemplate(string templateCode)
    {
        // Built-in templates
        return templateCode.ToLowerInvariant() switch
        {
            "automation_alert" => new EmailTemplate
            {
                Subject = "[SmartWMS] Automation Alert: {{ruleName}}",
                Body = @"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Automation Alert</h2>
                        <p><strong>Rule:</strong> {{ruleName}}</p>
                        <p><strong>Trigger:</strong> {{triggerType}}</p>
                        <p><strong>Entity:</strong> {{entityType}} - {{entityId}}</p>
                        <p><strong>Time:</strong> {{timestamp}}</p>
                        <hr/>
                        <p>{{message}}</p>
                        <hr/>
                        <p style='color: #666; font-size: 12px;'>
                            This is an automated message from SmartWMS.
                        </p>
                    </body>
                    </html>",
                IsHtml = true
            },
            "low_stock_alert" => new EmailTemplate
            {
                Subject = "[SmartWMS] Low Stock Alert: {{sku}}",
                Body = @"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Low Stock Alert</h2>
                        <p>Product <strong>{{sku}}</strong> ({{productName}}) is running low on stock.</p>
                        <table style='border-collapse: collapse;'>
                            <tr><td style='padding: 5px; border: 1px solid #ddd;'>Current Quantity:</td><td style='padding: 5px; border: 1px solid #ddd;'>{{currentQuantity}}</td></tr>
                            <tr><td style='padding: 5px; border: 1px solid #ddd;'>Minimum Level:</td><td style='padding: 5px; border: 1px solid #ddd;'>{{minLevel}}</td></tr>
                            <tr><td style='padding: 5px; border: 1px solid #ddd;'>Location:</td><td style='padding: 5px; border: 1px solid #ddd;'>{{locationCode}}</td></tr>
                        </table>
                        <p>Please review and take action.</p>
                    </body>
                    </html>",
                IsHtml = true
            },
            "order_status" => new EmailTemplate
            {
                Subject = "[SmartWMS] Order {{orderNumber}} - {{status}}",
                Body = @"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Order Status Update</h2>
                        <p>Order <strong>{{orderNumber}}</strong> status has been updated to <strong>{{status}}</strong>.</p>
                        <p><strong>Customer:</strong> {{customerName}}</p>
                        <p><strong>Updated:</strong> {{timestamp}}</p>
                    </body>
                    </html>",
                IsHtml = true
            },
            "report_delivery" => new EmailTemplate
            {
                Subject = "[SmartWMS] Report: {{reportType}}",
                Body = @"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>{{reportType}} Report</h2>
                        <p>Please find the attached report generated at {{timestamp}}.</p>
                        <p><strong>Report Type:</strong> {{reportType}}</p>
                        <p><strong>Period:</strong> {{dateFrom}} - {{dateTo}}</p>
                        <hr/>
                        <p style='color: #666; font-size: 12px;'>
                            This report was automatically generated by SmartWMS Automation.
                        </p>
                    </body>
                    </html>",
                IsHtml = true
            },
            _ => null
        };
    }

    private class EmailTemplate
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
    }
}
