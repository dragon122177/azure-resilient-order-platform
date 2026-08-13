using OrderGrid.Application.Messaging;
namespace OrderGrid.Worker;
public interface IEventRouter
{ Task RouteAsync(EventEnvelope envelope, CancellationToken cancellationToken); }
