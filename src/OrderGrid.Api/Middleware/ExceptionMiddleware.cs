using Microsoft.EntityFrameworkCore;
using OrderGrid.Application.Common;
using OrderGrid.Domain.Common;
namespace OrderGrid.Api.Middleware;
public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception) { await WriteAsync(context, exception); }
    }

    private async Task WriteAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail) = exception switch
        {
            BadHttpRequestException => (400, "Invalid request", "The request body or parameters could not be parsed."),
            RequestValidationException => (400, "Invalid request", exception.Message),
            ResourceNotFoundException => (404, "Resource not found", exception.Message),
            ConflictException => (409, "Conflict", exception.Message),
            DbUpdateConcurrencyException => (409, "Concurrent update", "The resource changed. Retry with fresh data."),
            DbUpdateException => (409, "Persistence conflict", "The change conflicts with existing data."),
            DomainException => (422, "Business rule rejected", exception.Message),
            _ => (500, "Unexpected error", "An unexpected error occurred.")
        };
        if (status >= 500) logger.LogError(exception, "Unhandled request error {TraceId}", context.TraceIdentifier);
        else logger.LogWarning(exception, "Request rejected with {StatusCode}", status);
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = context.TraceIdentifier,
            ["correlationId"] = context.Response.Headers["X-Correlation-ID"].FirstOrDefault()
        };
        if (exception is RequestValidationException validation) extensions["errors"] = validation.Errors;
        context.Response.StatusCode = status;
        await Results.Problem(statusCode: status, title: title, detail: detail,
            extensions: extensions).ExecuteAsync(context);
    }
}
