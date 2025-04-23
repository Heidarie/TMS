using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TMS.Application.Tasks.Repositories;
using TMS.Domain.Tasks.Entities;
using TMS.Domain.Tasks.Enums;
using TMS.Infrastructure.EF;
using TMS.Infrastructure.Tasks.Repositories;

namespace TMS.Tests;

public class TaskRepositoryTests
{
    private readonly ITaskRepository _sut;
    private readonly TasksDbContext _dbContext;
    private readonly Mock<ILogger<TaskRepository>> _loggerMock = new();

    public TaskRepositoryTests()
    {
        _dbContext = CreateDbContext();
        _sut = new TaskRepository(_dbContext, _loggerMock.Object);
    }

    private TasksDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseInMemoryDatabase($"TestDatabase_{Guid.NewGuid()}")
            .Options;
        return new TasksDbContext(options);
    }

    [Fact]
    public async Task CreateTaskAsync_ShouldAddTask()
    {
        // Arrange
        var task = new TaskItem(1, "Test Task", "Test Description", Status.NotStarted);

        // Act
        await _sut.CreateTaskAsync(task);

        // Assert
        var savedTask = _dbContext.TaskItems.FirstOrDefault(t => t.Id == task.Id);
        Assert.NotNull(savedTask);
        Assert.Equal(task.Name, savedTask.Name);
        Assert.Equal(task.Description, savedTask.Description);
        Assert.Equal(task.Status, savedTask.Status);
    }

    [Fact]
    public async Task CreateTaskAsync_DbNotAvailable_ShouldThrowException()
    {
        // Arrange
        var dbMock = new Mock<TasksDbContext>(new DbContextOptions<TasksDbContext>());
        var task = new TaskItem(1, "Test Task", "Test Description", Status.NotStarted);
        var taskRepo = new TaskRepository(dbMock.Object, _loggerMock.Object);

        dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("Database not available"));

        // Act, Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await taskRepo.CreateTaskAsync(task));
    }

    [Fact]
    public async Task GetAllTasksAsync_ShouldReturnAllTasks()
    {
        // Arrange
        var task1 = new TaskItem(1, "Task 1", "Description 1", Status.NotStarted);
        var task2 = new TaskItem(2, "Task 2", "Description 2", Status.InProgress);
        await _dbContext.TaskItems.AddRangeAsync(task1, task2);
        await _dbContext.SaveChangesAsync();

        // Act
        var tasks = await _sut.GetAllTasksAsync();

        // Assert
        Assert.Equal(2, tasks.Count());
    }

    [Fact]
    public async Task GetTaskByIdAsync_ShouldReturnTask()
    {
        // Arrange
        var task = new TaskItem(1, "Test Task", "Test Description", Status.NotStarted);
        await _sut.CreateTaskAsync(task);

        // Act
        var result = await _sut.GetTaskByIdAsync(task.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(task.Name, result.Name);
        Assert.Equal(task.Description, result.Description);
    }

    [Fact]
    public async Task GetTaskByIdAsync_TaskNotFound_ShouldReturnNull()
    {
        // Arrange
        var taskId = 999;

        // Act
        var result = await _sut.GetTaskByIdAsync(taskId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_ShouldUpdateTask()
    {
        // Arrange
        var task = new TaskItem(1, "Test Task", "Test Description", Status.NotStarted);
        await _sut.CreateTaskAsync(task);
        task.UpdateProgress();

        // Act
        await _sut.UpdateTaskAsync();

        // Assert
        var updatedTask = await _dbContext.TaskItems.FindAsync(task.Id);
        Assert.NotNull(updatedTask);
        Assert.Equal(Status.InProgress, updatedTask.Status);
    }

    [Fact]
    public async Task UpdateTaskAsync_DbNotAvailable_ShouldThrowException()
    {
        // Arrange
        var dbMock = new Mock<TasksDbContext>(new DbContextOptions<TasksDbContext>());
        var taskRepo = new TaskRepository(dbMock.Object, _loggerMock.Object);
        dbMock.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Throws(new DbUpdateException("Database not available"));
        
        // Act, Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await taskRepo.UpdateTaskAsync());
    }
}
