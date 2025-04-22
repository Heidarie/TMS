using Microsoft.Extensions.DependencyInjection;
using TMS.Application.Kernel;
using TMS.Application.Tasks.Repositories;
using TMS.Application.Tasks.Services;
using TMS.Domain.Tasks.DomainEvents;
using TMS.Infrastructure.Tasks.DomainEvents.Handlers;
using TMS.Infrastructure.Tasks.Repositories;
using TMS.Infrastructure.Tasks.Services;

namespace TMS.Infrastructure.Tasks;

public static class Extensions
{
    public static IServiceCollection AddTasks(this IServiceCollection services)
    {
        services
            .AddScoped<ITaskService, TaskService>()
            .AddScoped<IDomainEventHandler<TaskCompleted>, TaskCompletedHandler>()
            .AddScoped<ITaskRepository, TaskRepository>();
     
        return services;
    }
}