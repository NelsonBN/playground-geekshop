using System.Text.Json;
using GeekShop.Api.Products.AddProduct;

namespace GeekShop.Api.Tests.Messaging;

public sealed class IntegrationEventTests
{
    [Fact]
    public void Given_Event_When_ReadSubject_Then_IsTheEventName()
    {
        // Arrange
        var integrationEvent = new ProductAdded(Guid.NewGuid(), "Keyboard", 5, 19.9);

        // Act
        var subject = integrationEvent.Subject;

        // Assert
        subject.Should().Be(nameof(ProductAdded));
    }

    [Fact]
    public void Given_PublishedEvent_When_Deserialized_Then_KeepsItsEventIdAndOccurredAt()
    {
        // Arrange
        var published = new ProductAdded(Guid.NewGuid(), "Keyboard", 5, 19.9);
        var body = JsonSerializer.Serialize(published, JsonSerializerOptions.Web);

        // Act
        var received = JsonSerializer.Deserialize<ProductAdded>(body, JsonSerializerOptions.Web)!;

        // Assert
        received.EventId.Should().Be(published.EventId);
        received.OccurredAt.Should().Be(published.OccurredAt);
    }
}
