using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TMS.Application.Kernel;
using TMS.Application.Tasks.DTOs;
using TMS.Application.Tasks.Mappers;
using TMS.Application.Tasks.Services;
using TMS.Domain.Tasks.Entities;
using TMS.Infrastructure.EF;

namespace TMS.Infrastructure.Tasks.Services;

class TaskService(TasksDbContext dbContext, IDomainEventDispatcher domainEventDispatcher) : ITaskService
{
    DbSet<TaskItem> Tasks => dbContext.TaskItems;

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Task name cannot be empty.", nameof(dto.Name));
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            throw new ArgumentException("Task description cannot be empty.", nameof(dto.Description));
        }

        var taskItem = TaskItem.Create(dto.Name, dto.Description);

        Tasks.Add(taskItem);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to create task.", ex);
        }

        return TaskDtoMapper.ToDto(taskItem);
    }

    public async Task<TaskDto> GetTaskByIdAsync(int id)
    {
        TaskItem? taskItem = null;

        try
        {
            taskItem = await Tasks.SingleOrDefaultAsync(x => x.Id == id);
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Failed to retrieve task.", ex);
        }

        if (taskItem == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} not found.");
        }

        return TaskDtoMapper.ToDto(taskItem);
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        IEnumerable<TaskItem> tasks;

        try
        {
            tasks = await Tasks.ToListAsync();
        }
        catch (DbException ex)
        {
            throw new InvalidOperationException("Failed to retrieve tasks.", ex);
        }

        return tasks.Select(t => TaskDtoMapper.ToDto(t!));
    }

    public async Task<TaskDto> UpdateTaskAsync(int id)
    {
        try
        {
            var taskItem = await Tasks.SingleOrDefaultAsync(x => x.Id == id);

            if (taskItem == null)
                throw new KeyNotFoundException($"Task with ID {id} not found.");

            taskItem.UpdateProgress();

            await dbContext.SaveChangesAsync();

            await domainEventDispatcher.DispatchAsync(taskItem.DomainEvents.ToArray());

            return TaskDtoMapper.ToDto(taskItem);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to update task.", ex);
        }
    }
}