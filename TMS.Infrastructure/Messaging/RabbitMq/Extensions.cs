using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using TMS.Application.Messaging;

namespace TMS.Infrastructure.Messaging.RabbitMq;

public static class Extensions
{
    public static IServiceCollection AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        
        var options = configuration.GetSection("RabbitMq");
        services.AddSingleton<RabbitMqOptions>(opt =>
        {
            var rabbitMqOptions = new RabbitMqOptions();
            options.Bind(rabbitMqOptions, o =>
            {
                o.BindNonPublicProperties = true;
            });

            return rabbitMqOptions;
        });

        services.AddSingleton<ConnectionFactory>(sp => new ConnectionFactory
        {
            HostName = options["HostName"]!,
            UserName = options["UserName"]!,
            Password = options["Password"]!,
            Port = int.Parse(options["Port"]!),
        });

        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();

        return services;
    }
}