using OrderGrid.Application.Messaging;
namespace OrderGrid.Application.Abstractions;
public interface IEventPublisher
{
    Task PublishAsync(EventEnvelope envelope, CancellationToken cancellationToken);
}
