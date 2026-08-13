namespace OrderGrid.Domain.Operations;

public sealed class InboxMessage
{
    private InboxMessage() { }
    public InboxMessage(string consumer, string messageId, DateTimeOffset processedAt)
    { Consumer = consumer; MessageId = messageId; ProcessedAt = processedAt; }
    public string Consumer { get; private set; } = string.Empty;
    public string MessageId { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; private set; }
}
