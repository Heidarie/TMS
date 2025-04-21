using TMS.Application.Tasks.DTOs;

namespace TMS.Application.Tasks.Services;

public interface ITaskService
{
    Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
    Task<TaskDto> GetTaskByIdAsync(int id);
    Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    Task<TaskDto> UpdateTaskAsync(int id);
}