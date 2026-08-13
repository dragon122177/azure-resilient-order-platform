namespace OrderGrid.Application.Operations;
public interface IIdempotencyService
{
    Task<(T Value, bool Replayed)> ExecuteAsync<T>(string key, object request,
        Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);
}
