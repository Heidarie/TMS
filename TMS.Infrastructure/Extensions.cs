﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using TMS.Infrastructure.EF;
using TMS.Infrastructure.Kernel;
using TMS.Infrastructure.Messaging;
using TMS.Infrastructure.Tasks;

[assembly: InternalsVisibleTo("TMS.Tests")]
namespace TMS.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddTasksDbContext(configuration.GetConnectionString("PostgreSQL")!)
            .AddMessaging(configuration)
            .AddDomainEventDispatcher()
            .AddTasks();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder)
    {
        builder.MigrateDbContext();
        return builder;
    }
}