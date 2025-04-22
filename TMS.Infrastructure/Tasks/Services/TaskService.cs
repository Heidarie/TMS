using TMS.Application.Kernel;
using TMS.Application.Tasks.DTOs;
using TMS.Application.Tasks.Mappers;
using TMS.Application.Tasks.Repositories;
using TMS.Application.Tasks.Services;
using TMS.Domain.Tasks.Entities;

namespace TMS.Infrastructure.Tasks.Services;

class TaskService(ITaskRepository taskRepository, IDomainEventDispatcher domainEventDispatcher) : ITaskService
{
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

        var taskEntity = await taskRepository.CreateTaskAsync(taskItem);

        return TaskDtoMapper.ToDto(taskEntity);
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await taskRepository.GetAllTasksAsync();
        return tasks.Select(t => TaskDtoMapper.ToDto(t!));
    }

    public async Task<TaskDto> UpdateTaskAsync(int id)
    {
        var taskItem = await taskRepository.GetTaskByIdAsync(id);

        if (taskItem == null)
        {
            throw new KeyNotFoundException($"Task with ID {id} not found.");
        }

        taskItem.UpdateProgress();

        await taskRepository.UpdateTaskAsync();

        await domainEventDispatcher.DispatchAsync(taskItem.DomainEvents.ToArray());

        return TaskDtoMapper.ToDto(taskItem);
    }
}