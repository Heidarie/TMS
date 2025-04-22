using TMS.Application.Tasks.DTOs;

namespace TMS.Application.Tasks.Services;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
    Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    Task<TaskDto> UpdateTaskAsync(int id);
}