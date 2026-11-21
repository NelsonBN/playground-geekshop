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
}
