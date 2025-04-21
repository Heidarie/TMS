using Microsoft.Extensions.DependencyInjection;
using TMS.Application.Kernel;

namespace TMS.Infrastructure.Kernel;

public static class Extensions
{
    public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection services)
    {
        services.AddSingleton<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}