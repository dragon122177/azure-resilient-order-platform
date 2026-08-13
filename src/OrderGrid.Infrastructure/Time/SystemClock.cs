using OrderGrid.Application.Abstractions;
namespace OrderGrid.Infrastructure.Time;
public sealed class SystemClock : IClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }
