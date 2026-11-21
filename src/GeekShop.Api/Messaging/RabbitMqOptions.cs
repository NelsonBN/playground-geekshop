namespace GeekShop.Api.Messaging;

public sealed class RabbitMqOptions
{
    public const string SECTION = "RabbitMq";

    /// <summary>AMQP URI of the broker, for example <c>amqp://guest:guest@localhost:5672</c>.</summary>
    public string ConnectionString { get; init; } = string.Empty;
}
