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
            HostName = options["HostName"] ?? throw new InvalidOperationException("RabbitMQ HostName configuration is missing."),
            UserName = options["UserName"] ?? throw new InvalidOperationException("RabbitMQ UserName configuration is missing."),
            Password = options["Password"] ?? throw new InvalidOperationException("RabbitMQ Password configuration is missing."),
            Port = int.Parse(options["Port"] ?? throw new InvalidOperationException("RabbitMQ Port configuration is missing.")),
        });

        services.AddSingleton<IMessageBroker, RabbitMqMessageBroker>();

        return services;
    }
}