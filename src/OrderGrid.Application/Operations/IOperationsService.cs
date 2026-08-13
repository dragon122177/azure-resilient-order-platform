namespace OrderGrid.Application.Operations;
public interface IOperationsService
{ Task<IReadOnlyList<AuditResponse>> ListAuditAsync(int limit, CancellationToken cancellationToken); }
