namespace OrderGrid.Application.Abstractions;
public interface IRequestContext
{
    string TenantId { get; }
    string Actor { get; }
    string CorrelationId { get; }
}
