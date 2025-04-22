using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMS.Infrastructure.Messaging.RabbitMq;

namespace TMS.Infrastructure.Messaging;

internal static class Extensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRabbitMq(configuration)
            .AddHostedService<CompletedTaskMessageConsumerBackgroundService>();

        return services;
    }
}