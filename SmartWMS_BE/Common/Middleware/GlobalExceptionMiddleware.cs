using System.Net;
using System.Text.Json;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Common.Middleware;

/// <summary>
/// Global exception handler middleware.
/// Catches all unhandled exceptions and returns a consistent error response.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.TraceIdentifier;

        // Log the exception with structured data
        _logger.LogError(exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, User: {User}",
            correlationId,
            context.Request.Path,
            context.Request.Method,
            context.User.Identity?.Name ?? "anonymous");

        context.Response.ContentType = "application/json";

        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "ARGUMENT_NULL", exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Access denied"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "INVALID_OPERATION", exception.Message),
            TimeoutException => (HttpStatusCode.RequestTimeout, "TIMEOUT", "The operation timed out"),
            NotImplementedException => (HttpStatusCode.NotImplemented, "NOT_IMPLEMENTED", "This feature is not yet implemented"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred")
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new ErrorResponse
        {
            Success = false,
            CorrelationId = correlationId,
            Error = new ErrorDetails
            {
                Code = errorCode,
                Message = message,
                // Only include stack trace in development
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                InnerError = _environment.IsDevelopment() && exception.InnerException != null
                    ? exception.InnerException.Message
                    : null
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}

/// <summary>
/// Error response DTO
/// </summary>
public class ErrorResponse
{
    public bool Success { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public ErrorDetails Error { get; set; } = new();
}

/// <summary>
/// Error details
/// </summary>
public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerError { get; set; }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
