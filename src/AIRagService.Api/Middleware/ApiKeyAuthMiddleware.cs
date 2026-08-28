using AIRagService.Application.Configuration;
using Microsoft.Extensions.Options;

namespace AIRagService.Api.Middleware;

public class ApiKeyAuthMiddleware(RequestDelegate next, IOptions<ApiKeyAuthOptions> options)
{
    private readonly ApiKeyAuthOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled || IsPublicPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var providedKey)
            || string.IsNullOrWhiteSpace(providedKey)
            || !string.Equals(providedKey, _options.ApiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { title = "Unauthorized", detail = "Invalid or missing API key." });
            return;
        }

        await next(context);
    }

    private static bool IsPublicPath(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/health", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
            || value == "/"
            || value.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);
    }
}
