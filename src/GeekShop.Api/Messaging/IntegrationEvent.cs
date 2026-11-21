using System.Text.Json.Serialization;

namespace GeekShop.Api.Messaging;

/// <summary>
/// Base of every integration event: a fact other systems may react to. The topic and the event name (the type name)
/// route the message and travel outside the body, as the exchange and the routing key.
/// </summary>
public abstract record IntegrationEvent(
    [property: JsonIgnore] string Topic)
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    [JsonIgnore]
    public string Subject => GetType().Name;
}
