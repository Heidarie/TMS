using Microsoft.EntityFrameworkCore;
using TMS.Domain.Tasks.Entities;

namespace TMS.Infrastructure.EF;

public class TasksDbContext : DbContext
{
    public DbSet<TaskItem> TaskItems { get; set; } = null!;

    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tasks");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}