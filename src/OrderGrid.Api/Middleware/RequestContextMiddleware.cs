using System.Security.Claims;
using OrderGrid.Infrastructure.Context;
namespace OrderGrid.Api.Middleware;
public sealed class RequestContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, RequestContext context)
    {
        var correlation = http.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlation)) correlation = Guid.NewGuid().ToString("N");
        var entra = string.Equals(http.RequestServices.GetRequiredService<IConfiguration>()["Authentication:Mode"],
            "EntraId", StringComparison.OrdinalIgnoreCase);
        var tenant = http.User.FindFirstValue("tenant_id") ?? http.User.FindFirstValue("tid");
        if (entra && http.User.Identity?.IsAuthenticated == true && string.IsNullOrWhiteSpace(tenant))
        {
            http.Response.StatusCode = StatusCodes.Status403Forbidden;
            http.Response.Headers["X-Correlation-ID"] = correlation;
            await Results.Problem(statusCode: 403, title: "Tenant identity required",
                detail: "The validated access token does not contain a tenant claim.",
                extensions: new Dictionary<string, object?> { ["correlationId"] = correlation }).ExecuteAsync(http);
            return;
        }
        tenant ??= http.Request.Headers["X-Tenant-ID"].FirstOrDefault() ?? "demo";
        var actor = http.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? http.User.FindFirstValue("sub") ?? http.User.Identity?.Name ?? "anonymous";
        context.Set(tenant, actor, correlation);
        http.Response.Headers["X-Correlation-ID"] = context.CorrelationId;
        await next(http);
    }
}
