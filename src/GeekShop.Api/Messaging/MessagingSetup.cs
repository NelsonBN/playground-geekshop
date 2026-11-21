namespace GeekShop.Api.Messaging;

public static class MessagingSetup
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SECTION))
            .AddSingleton<IRabbitMqConnection, RabbitMqConnection>()
            .AddSingleton<IEventPublisher, RabbitMqEventPublisher>()
            .AddHostedService<RabbitMqExchangesInitializer>();
}
