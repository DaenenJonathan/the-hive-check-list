using System.Text.Json;
using TheHive.Application.Common.Exceptions;
using TheHive.Domain.Exceptions;

namespace TheHive.WebApi.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await HandleExceptionAsync(context, ex, _env.IsDevelopment());
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException ve => (400, "Validation error", ve.Errors.SelectMany(e => e.Value).ToArray()),
            NotFoundException nfe => (404, nfe.Message, Array.Empty<string>()),
            ForbiddenAccessException => (403, "Access denied", Array.Empty<string>()),
            DomainException de => (422, de.Message, Array.Empty<string>()),
            _ when isDevelopment => (500, $"{exception.GetType().Name}: {exception.Message}", Array.Empty<string>()),
            _ => (500, "An unexpected error occurred", Array.Empty<string>())
        };

        context.Response.StatusCode = statusCode;

        var response = JsonSerializer.Serialize(new
        {
            status = statusCode,
            message,
            errors
        });

        return context.Response.WriteAsync(response);
    }
}
