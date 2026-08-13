using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OrderGrid.Application.Abstractions;
namespace OrderGrid.Infrastructure.Storage;
public sealed class BlobReceiptStore(BlobServiceClient client, InfrastructureOptions options) : IReceiptStore
{
    public async Task<Uri> StoreAsync(string tenant, Guid orderId, Stream content,
        string contentType, CancellationToken token)
    {
        var container = client.GetBlobContainerClient(options.ReceiptContainer);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: token);
        var blob = container.GetBlobClient($"{tenant}/{orderId:N}.json");
        await blob.UploadAsync(content, new BlobUploadOptions
        { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } }, token);
        return blob.Uri;
    }
}
