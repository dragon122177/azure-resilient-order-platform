using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OrderGrid.Application.Abstractions;
using OrderGrid.Application.Common;
using OrderGrid.Domain.Operations;

namespace OrderGrid.Application.Operations;

public sealed class IdempotencyService(IOperationsRepository operations, IUnitOfWork unitOfWork,
    IRequestContext context, IClock clock) : IIdempotencyService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<(T Value, bool Replayed)> ExecuteAsync<T>(string key, object request,
        Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        var normalized = key?.Trim() ?? string.Empty;
        if (normalized.Length is < 8 or > 160)
            throw new RequestValidationException(new Dictionary<string, string[]>
            { ["Idempotency-Key"] = ["Idempotency-Key must contain between 8 and 160 characters."] });

        var requestHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(request, JsonOptions))));
        var existing = await operations.GetIdempotencyAsync(context.TenantId, normalized, cancellationToken);
        if (existing is not null)
        {
            if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(existing.RequestHash),
                Encoding.ASCII.GetBytes(requestHash)))
                throw new ConflictException("The idempotency key was already used for a different request.");
            return (JsonSerializer.Deserialize<T>(existing.ResponseBody, JsonOptions)
                ?? throw new InvalidOperationException("Stored response could not be deserialized."), true);
        }

        T? value = default;
        await unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            value = await action(token);
            operations.AddIdempotency(new IdempotencyRecord(context.TenantId, normalized,
                requestHash, 201, JsonSerializer.Serialize(value, JsonOptions),
                clock.UtcNow, clock.UtcNow.AddHours(24)));
            await unitOfWork.SaveChangesAsync(token);
        }, cancellationToken);
        return (value!, false);
    }
}
