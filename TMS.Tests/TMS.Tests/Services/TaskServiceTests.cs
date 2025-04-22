using Moq;
using TMS.Application.Kernel;
using TMS.Application.Tasks.DTOs;
using TMS.Application.Tasks.Repositories;
using TMS.Application.Tasks.Services;
using TMS.Domain.Kernel;
using TMS.Domain.Tasks.Entities;
using TMS.Domain.Tasks.Enums;
using TMS.Infrastructure.Tasks.Services;
using TMS.Tests.Builders;

namespace TMS.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskService _sut;
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly Mock<IDomainEventDispatcher> _taskDispatcherMock = new();
    private TaskDtoBuilder _taskDtoBuilder = new();

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepositoryMock.Object, _taskDispatcherMock.Object);
    }

    [Fact]
    public async Task GetAllTasks_ShouldReturnAllTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new(1, "Test", "Test Description", Status.NotStarted),
            new(2, "Test", "Test Description", Status.NotStarted)
        };

        var tasksDto1 = _taskDtoBuilder.WithId(1).WithDescription("Test Description").WithName("Test").Build();
        var tasksDto2 = _taskDtoBuilder.WithId(2).WithDescription("Test Description").WithName("Test").Build();

        var tasksDtos = new List<TaskDto>
        {
            tasksDto1,
            tasksDto2
        };

        _taskRepositoryMock.Setup(repo => repo.GetAllTasksAsync()).ReturnsAsync(tasks);

        // Act
        var result = await _sut.GetAllTasksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());

        for(int i = 0; i < tasks.Count; i++)
        {
            Assert.Equal(tasksDtos[i].Id, result!.ElementAt(i).Id);
            Assert.Equal(tasksDtos[i].Name, result!.ElementAt(i).Name);
            Assert.Equal(tasksDtos[i].Description, result!.ElementAt(i).Description);
        }
    }

    [Fact]
    public async Task CreateTask_ShouldCreateTask()
    {
        // Arrange
        var taskDto = new CreateTaskDto("Test", "Test Description");

        var taskItem = new TaskItem(1, taskDto.Name, taskDto.Description, Status.NotStarted);

        _taskRepositoryMock.Setup(repo => repo.CreateTaskAsync(It.IsAny<TaskItem>()))
            .Returns(Task.FromResult(taskItem));
        
        // Act
        var result = await _sut.CreateTaskAsync(taskDto);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Name, result.Name);
        Assert.Equal(taskDto.Description, result.Description);
    }

    [Fact]
    public async Task CreateTask_EmptyName_ShouldThrowException()
    {
        // Arrange
        var taskDto = new CreateTaskDto("", "Test Description");

        // Act, Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.CreateTaskAsync(taskDto));
    }

    [Fact]
    public async Task CreateTask_EmptyDescription_ShouldThrowException()
    {
        // Arrange
        var taskDto = new CreateTaskDto("Test", "");

        // Act, Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.CreateTaskAsync(taskDto));
    }

    [Fact]
    public async Task UpdateTask_TaskNotStarted_ShouldUpdateTask_NoDomainEventSent()
    {
        // Arrange
        var taskItem = new TaskItem(1, "Test", "Test Description", Status.NotStarted);
        var taskDto = _taskDtoBuilder.WithId(1).WithDescription("Test Description").WithName("Test").Build();

        _taskRepositoryMock.Setup(repo => repo.GetTaskByIdAsync(It.IsAny<int>()))
            .Returns(Task.FromResult(taskItem))
            .Verifiable();

        _taskRepositoryMock.Setup(repo => repo.UpdateTaskAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        _taskDispatcherMock.Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<IDomainEvent[]>()))
            .Verifiable();

        // Act
        var result = await _sut.UpdateTaskAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Id, result.Id);
        Assert.Equal(taskDto.Name, result.Name);
        Assert.Equal(taskDto.Description, result.Description);

        _taskRepositoryMock.Verify();
        _taskDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTask_TaskInProgress_ShouldUpdateTask_SingleDomainEventSent()
    {
        // Arrange
        var taskItem = new TaskItem(1, "Test", "Test Description", Status.InProgress);
        var taskDto = _taskDtoBuilder.WithId(1).WithDescription("Test Description").WithName("Test").WithStatus(Status.InProgress).Build();

        _taskRepositoryMock.Setup(repo => repo.GetTaskByIdAsync(It.IsAny<int>()))
            .Returns(Task.FromResult(taskItem))
            .Verifiable();

        _taskRepositoryMock.Setup(repo => repo.UpdateTaskAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        _taskDispatcherMock.Setup(dispatcher => dispatcher.DispatchAsync(It.IsAny<IDomainEvent[]>()))
            .Verifiable();

        // Act
        var result = await _sut.UpdateTaskAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(taskDto.Id, result.Id);
        Assert.Equal(taskDto.Name, result.Name);
        Assert.Equal(taskDto.Description, result.Description);

        _taskRepositoryMock.Verify();
        _taskDispatcherMock.Verify(d => d.DispatchAsync(It.IsAny<IDomainEvent>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_TaskCompleted_ShouldThrowException()
    {
        // Arrange
        var taskItem = new TaskItem(1, "Test", "Test Description", Status.Completed);

        _taskRepositoryMock.Setup(repo => repo.GetTaskByIdAsync(It.IsAny<int>()))
            .Returns(Task.FromResult(taskItem))
            .Verifiable();

        // Act, Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _sut.UpdateTaskAsync(1));
    }
}