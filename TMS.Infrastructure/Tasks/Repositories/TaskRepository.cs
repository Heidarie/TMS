using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using TMS.Application.Tasks.Repositories;
using TMS.Domain.Tasks.Entities;
using TMS.Infrastructure.EF;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace TMS.Infrastructure.Tasks.Repositories;

internal class TaskRepository(TasksDbContext dbContext, ILogger<TaskRepository> logger) : ITaskRepository
{
    DbSet<TaskItem> Tasks => dbContext.TaskItems;

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync() => await Tasks.ToListAsync();

    public Task<TaskItem?> GetTaskByIdAsync(int id) => Tasks.SingleOrDefaultAsync(x => x.Id == id);

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        try
        {
            await Tasks.AddAsync(task);
            await dbContext.SaveChangesAsync();
            return task;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create task.");
            throw new InvalidOperationException("Failed to create task.", ex);
        }
    }

    public Task UpdateTaskAsync()
    {
        try
        {
            return dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Failed to update task.");
            throw new InvalidOperationException("Failed to update task.", ex);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update task.");
            throw new InvalidOperationException("Failed to update task.", ex);
        }
    }
}