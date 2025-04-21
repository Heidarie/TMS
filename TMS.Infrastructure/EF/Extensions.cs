using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TMS.Infrastructure.EF;

internal static class Extensions
{
    public static IServiceCollection AddTasksDbContext(this IServiceCollection services, string dbConnectionString)
    {
        services
            .AddDbContext<TasksDbContext>(opt =>
                opt.UseNpgsql(dbConnectionString));
        return services;
    }

    public static IApplicationBuilder MigrateDbContext(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();
        return builder;
    }
}