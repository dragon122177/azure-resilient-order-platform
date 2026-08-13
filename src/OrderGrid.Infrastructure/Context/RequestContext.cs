using OrderGrid.Application.Abstractions;
namespace OrderGrid.Infrastructure.Context;
public sealed class RequestContext : IRequestContext
{
    public string TenantId { get; private set; } = "demo";
    public string Actor { get; private set; } = "system";
    public string CorrelationId { get; private set; } = Guid.NewGuid().ToString("N");

    public void Set(string tenantId, string actor, string correlationId)
    {
        TenantId = Normalize(tenantId, "demo", 64);
        Actor = Normalize(actor, "system", 160);
        CorrelationId = Normalize(correlationId, Guid.NewGuid().ToString("N"), 128);
    }

    private static string Normalize(string? value, string fallback, int max)
    {
        var result = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return result.Length > max ? result[..max] : result;
    }
}
