using OrderGrid.Application.Common;
namespace OrderGrid.Application.Tests;
public sealed class IdempotencyServiceTests
{
    [Fact] public async Task Replays_stored_result_without_reexecuting_action()
    { await using var f = await ApplicationFixture.CreateAsync(); var calls = 0;
      Task<string> Action(CancellationToken _) { calls++; return Task.FromResult("created"); }
      var first = await f.Idempotency.ExecuteAsync("valid-key-001", new { value = 1 }, Action, default);
      var second = await f.Idempotency.ExecuteAsync("valid-key-001", new { value = 1 }, Action, default);
      Assert.False(first.Replayed); Assert.True(second.Replayed); Assert.Equal(1, calls); }

    [Fact] public async Task Rejects_key_reuse_with_changed_payload()
    { await using var f = await ApplicationFixture.CreateAsync();
      await f.Idempotency.ExecuteAsync("valid-key-002", new { value = 1 }, _ => Task.FromResult("ok"), default);
      await Assert.ThrowsAsync<ConflictException>(() => f.Idempotency.ExecuteAsync(
          "valid-key-002", new { value = 2 }, _ => Task.FromResult("ok"), default)); }
}
