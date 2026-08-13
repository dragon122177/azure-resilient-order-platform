using OrderGrid.Application.Abstractions;
namespace OrderGrid.Infrastructure.Storage;
public sealed class LocalReceiptStore(InfrastructureOptions options) : IReceiptStore
{
    public async Task<Uri> StoreAsync(string tenant, Guid orderId, Stream content,
        string contentType, CancellationToken token)
    {
        var directory = Path.GetFullPath(options.LocalStoragePath); Directory.CreateDirectory(directory);
        var safe = string.Concat(tenant.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
        if (string.IsNullOrWhiteSpace(safe)) safe = "tenant";
        var path = Path.Combine(directory, $"{safe}-{orderId:N}.json");
        await using var file = File.Create(path); await content.CopyToAsync(file, token);
        return new Uri(path);
    }
}
