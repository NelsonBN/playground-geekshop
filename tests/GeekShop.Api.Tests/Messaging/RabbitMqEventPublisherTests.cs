using System.Text.Json;
using GeekShop.Api.Messaging;
using GeekShop.Api.Products.AddProduct;
using RabbitMQ.Client;

namespace GeekShop.Api.Tests.Messaging;

public sealed class RabbitMqEventPublisherTests
{
    private readonly IRabbitMqConnection _connection = Substitute.For<IRabbitMqConnection>();
    private readonly IConnection _broker = Substitute.For<IConnection>();
    private readonly IChannel _channel = Substitute.For<IChannel>();

    public RabbitMqEventPublisherTests()
    {
        _connection.GetAsync(Arg.Any<CancellationToken>()).Returns(_broker);
        _broker.CreateChannelAsync(Arg.Any<CreateChannelOptions?>(), Arg.Any<CancellationToken>()).Returns(_channel);
    }

    [Fact]
    public async Task Given_Event_When_Publish_Then_PublishesJsonToTheTopicExchangeRoutedBySubject()
    {
        // Arrange
        var publisher = new RabbitMqEventPublisher(_connection);
        var integrationEvent = new ProductAdded(Guid.NewGuid(), "Keyboard", 5, 19.9);

        // Act
        await publisher.PublishAsync(integrationEvent, CancellationToken.None);

        // Assert
        await _channel.Received(1).BasicPublishAsync(
            Topics.PRODUCTS,
            nameof(ProductAdded),
            false,
            Arg.Is<BasicProperties>(p => p.MessageId == integrationEvent.EventId.ToString()
                && p.Type == nameof(ProductAdded)
                && p.ContentType == "application/json"
                && p.DeliveryMode == DeliveryModes.Persistent),
            Arg.Is<ReadOnlyMemory<byte>>(b => JsonSerializer.Deserialize<ProductAdded>(b.ToArray(), JsonSerializerOptions.Web)!.Id == integrationEvent.Id),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_Event_When_Publish_Then_UsesAChannelWithPublisherConfirmations()
    {
        // Arrange
        var publisher = new RabbitMqEventPublisher(_connection);

        // Act
        await publisher.PublishAsync(new ProductAdded(Guid.NewGuid(), "A", 1, 1), CancellationToken.None);

        // Assert
        await _broker.Received(1).CreateChannelAsync(
            Arg.Is<CreateChannelOptions?>(o => o!.PublisherConfirmationsEnabled && o.PublisherConfirmationTrackingEnabled),
            CancellationToken.None);
    }

    [Fact]
    public async Task Given_PublishedEvent_When_PublishCompletes_Then_DisposesTheChannel()
    {
        // Arrange
        var publisher = new RabbitMqEventPublisher(_connection);

        // Act
        await publisher.PublishAsync(new ProductAdded(Guid.NewGuid(), "A", 1, 1), CancellationToken.None);

        // Assert
        await _channel.Received(1).DisposeAsync();
    }
}
