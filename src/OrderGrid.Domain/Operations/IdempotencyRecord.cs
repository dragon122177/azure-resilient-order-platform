namespace OrderGrid.Domain.Operations;

public sealed class IdempotencyRecord
{
    private IdempotencyRecord() { }
    public IdempotencyRecord(string tenantId, string key, string requestHash, int statusCode,
        string responseBody, DateTimeOffset createdAt, DateTimeOffset expiresAt)
    {
        TenantId = tenantId; Key = key; RequestHash = requestHash; StatusCode = statusCode;
        ResponseBody = responseBody; CreatedAt = createdAt; ExpiresAt = expiresAt;
    }
    public string TenantId { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public string RequestHash { get; private set; } = string.Empty;
    public int StatusCode { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
}
