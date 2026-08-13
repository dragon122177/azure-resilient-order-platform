using OrderGrid.Application.Abstractions;
namespace OrderGrid.Application.Tests;
internal sealed class TestClock(DateTimeOffset now) : IClock { public DateTimeOffset UtcNow { get; set; } = now; }
