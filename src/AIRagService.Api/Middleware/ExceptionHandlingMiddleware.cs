using System.Net;
using AIRagService.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AIRagService.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException validation => (HttpStatusCode.BadRequest, "Validation failed", validation.Message),
            NotFoundException notFound => (HttpStatusCode.NotFound, "Resource not found", notFound.Message),
            DuplicateDocumentException duplicate => (HttpStatusCode.Conflict, "Duplicate document", duplicate.Message),
            PdfProcessingException pdf => (HttpStatusCode.BadRequest, "PDF processing failed", pdf.Message),
            ExternalServiceException external => (HttpStatusCode.BadGateway, "External service error", external.Message),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "An internal server error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            logger.LogError(exception, "Unhandled exception for request {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning(exception, "Handled exception: {Title}", title);

        var problem = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{(int)statusCode}",
            Title = title,
            Status = (int)statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(problem);
    }
}
