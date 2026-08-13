namespace OrderGrid.Application.Abstractions;
public interface IReceiptStore
{
    Task<Uri> StoreAsync(string tenantId, Guid orderId, Stream content,
        string contentType, CancellationToken cancellationToken);
}
