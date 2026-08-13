namespace OrderGrid.Domain.Common;

public sealed class DomainException(string message) : Exception(message);
