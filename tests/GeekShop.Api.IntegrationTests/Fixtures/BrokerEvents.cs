using System.Text.Json;
using AwesomeAssertions.Equivalency;
using GeekShop.Api.Messaging;

namespace GeekShop.Api.IntegrationTests.Fixtures;

/// <summary>
/// What the endpoint tests ask of the real broker: the event an endpoint announced, or nothing at all.
/// </summary>
public static class BrokerEvents
{
    private static readonly TimeSpan _arrives = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan _neverArrives = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Starting the host creates the topics (the warm-up); only then can the subscription the tests read from exist,
    /// and it must exist before anything is published.
    /// </summary>
    public static async Task SubscribeAsync(this RabbitMqFixture broker, ApiFactory factory)
    {
        _ = factory.CreateClient();
        await broker.EnsureSubscriptionsAsync();
    }

    public static async Task<T> ExpectAsync<T>(this RabbitMqFixture broker, string topic, Guid id, string property = "id")
        where T : IntegrationEvent
    {
        var subject = typeof(T).Name;
        var received = await broker.ReceiveAsync(topic, e => e.Subject == subject && e.HasId(id, property), _arrives);

        received.Should().NotBeNull($"the broker should have received {subject} for {id}");
        received!.ContentType.Should().Be("application/json");
        received.MessageId.Should().NotBeEmpty();

        return received.Body.Deserialize<T>(JsonSerializerOptions.Web)!;
    }

    public static async Task ExpectNothingAsync(this RabbitMqFixture broker, string topic, string subject, Guid id, string property = "id")
    {
        var received = await broker.ReceiveAsync(topic, e => e.Subject == subject && e.HasId(id, property), _neverArrives);

        received.Should().BeNull($"{subject} for {id} must not be published");
    }

    /// <summary>EventId and OccurredAt are generated per event, so only the business payload is compared.</summary>
    public static EquivalencyOptions<T> IgnoreEnvelope<T>(EquivalencyOptions<T> options)
        => options.Excluding(member => member.Name == nameof(IntegrationEvent.EventId) || member.Name == nameof(IntegrationEvent.OccurredAt));
}
