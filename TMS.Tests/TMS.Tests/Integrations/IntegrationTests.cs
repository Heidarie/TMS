using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using TMS.Application.Tasks.DTOs;
using TMS.Application.Tasks.Messages;
using TMS.Domain.Tasks.Enums;
using TMS.Infrastructure.EF;
using TMS.Infrastructure.Messaging;
using TMS.Infrastructure.Messaging.RabbitMq;

namespace TMS.Tests.Integrations;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>,
    IAsyncLifetime
{

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("test_tms_db")
        .WithUsername("test")
        .WithPassword("test")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithCleanUp(true)
        .WithPortBinding(5672, true)
        .Build();

    private ConcurrentBag<TaskCompletedMessage> _receivedMessages = null!;

    private HttpClient _client = null!;
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _rabbitMq.StartAsync();

        var postgresConnStr = _postgres.GetConnectionString();
        var rabbitHost = _rabbitMq.Hostname;
        var rabbitPort = _rabbitMq.GetMappedPublicPort(5672);

        _receivedMessages = new ConcurrentBag<TaskCompletedMessage>();

        _client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var dict = new Dictionary<string, string?>
                {
                    { "ConnectionStrings:PostgreSQL", postgresConnStr },
                    { "RabbitMq:HostName", rabbitHost },
                    { "RabbitMq:Port", rabbitPort.ToString() },
                    { "RabbitMq:UserName", "guest" },
                    { "RabbitMq:Password", "guest" },
                    { "RabbitMq:ExchangeName", "tasks.exchange" },
                    { "RabbitMq:QueueName", "tasks.events" }
                };
                config.AddInMemoryCollection(dict!);
            });
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ImplementationType == typeof(CompletedTaskMessageConsumerBackgroundService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddLogging();
                services.AddSingleton(_receivedMessages);

                services.AddHostedService(sp => new CompletedTaskMessageConsumerBackgroundServiceTest(
                    sp.GetRequiredService<ConnectionFactory>(),
                    sp.GetRequiredService<RabbitMqOptions>(), sp.GetRequiredService<ILogger<CompletedTaskMessageConsumerBackgroundService>>(), _receivedMessages));

                services.AddTasksDbContext(postgresConnStr);
            });
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });
        }).CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _postgres.DisposeAsync().AsTask();
        await _rabbitMq.DisposeAsync().AsTask();
    }

    [Fact]
    public async Task GetTasks_ShouldReturnEmptyList()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var tasks = await response.Content.ReadFromJsonAsync<IEnumerable<TaskDto>>();
        Assert.NotNull(tasks);
        Assert.Empty(tasks);
    }

    [Fact]
    public async Task CreateTask_ShouldReturnCreatedTask()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto("Test", "Test");

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(createdTask);
        Assert.Equal(createTaskDto.Name, createdTask.Name);
        Assert.Equal(createTaskDto.Description, createdTask.Description);
    }

    [Fact]
    public async Task UpdateTask_ShouldReturnUpdatedTask()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto("Test", "Test");

        // Create a task
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createTaskDto);
        var createdTask = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        // Act
        var updateResponse = await _client.PutAsync($"/api/tasks/{createdTask!.Id}", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedTask = await updateResponse.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(updatedTask);
        Assert.Equal(createdTask.Id, updatedTask.Id);
        Assert.NotEqual(createdTask.Status, updatedTask.Status);
    }

    [Fact]
    public async Task FullPath_ShouldWork()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto("Test", "Test");

        // Act, Assert - Create a task
        var createTaskResponse = await _client.PostAsJsonAsync("/api/tasks", createTaskDto);
        Assert.Equal(HttpStatusCode.Created, createTaskResponse.StatusCode);
        var createdTask = await createTaskResponse.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(createdTask);
        Assert.Equal(createTaskDto.Name, createdTask.Name);
        Assert.Equal(createTaskDto.Description, createdTask.Description);

        // Act, Assert - Get all tasks
        var getAllResponse = await _client.GetAsync("/api/tasks");
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        var tasks = await getAllResponse.Content.ReadFromJsonAsync<List<TaskDto>>();
        Assert.NotNull(tasks);
        Assert.NotEmpty(tasks);
        Assert.Single(tasks);
        Assert.Equal(createdTask.Id, tasks.First().Id);
        Assert.Equal(createdTask.Name, tasks.First().Name);
        Assert.Equal(createdTask.Description, tasks.First().Description);

        //Act, Assert - Update the task first time
        var firstUpdateResponse = await _client.PutAsync($"/api/tasks/{createdTask.Id}", null);
        Assert.Equal(HttpStatusCode.OK, firstUpdateResponse.StatusCode);
        var firstUpdatedTask = await firstUpdateResponse.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(firstUpdatedTask);
        Assert.Equal(createdTask.Id, firstUpdatedTask.Id);
        Assert.NotEqual(createdTask.Status, firstUpdatedTask.Status);

        // Act, Assert - Update the task second time
        var secondUpdateResponse = await _client.PutAsync($"/api/tasks/{createdTask.Id}", null);
        var content = await secondUpdateResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, firstUpdateResponse.StatusCode);
        var secondUpdatedTask = await secondUpdateResponse.Content.ReadFromJsonAsync<TaskDto>();
        Assert.NotNull(secondUpdatedTask);
        Assert.Equal(createdTask.Id, secondUpdatedTask.Id);
        Assert.NotEqual(createdTask.Status, secondUpdatedTask.Status);
        Assert.Equal(Status.Completed.ToString(), secondUpdatedTask.Status);

        //Assert - Check if the message was received
        Assert.NotEmpty(_receivedMessages);
        var message = _receivedMessages.FirstOrDefault(m => m.Id == createdTask.Id);

        Assert.NotNull(message);
        Assert.Equal(createdTask.Id, message.Id);
        Assert.Equal(createdTask.Name, message.Name);
        Assert.Equal(createdTask.Description, message.Description);
    }
}