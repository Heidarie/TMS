using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using TMS.Application.Tasks.DTOs;
using TMS.Application.Tasks.Services;
using TMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() {Title = "Task Management System", Version = "v1"});
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseInfrastructure();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management System v1");
    });
}


app.MapGet("/api/tasks", async (ITaskService taskService) =>
{
    try
    {
        var tasks = await taskService.GetAllTasksAsync();
        return Results.Ok(tasks);
    }
    catch (DbException ex)
    {
        return Results.Problem(ex.Message);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/tasks", async (ITaskService taskService, CreateTaskDto dto) =>
{
    try
    {
        var task = await taskService.CreateTaskAsync(dto);
        return Results.Created($"/api/tasks/{task.Id}", task);
    }
    catch (DbUpdateException ex)
    {
        return Results.Problem(ex.Message);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/tasks/{id:int}", async (ITaskService taskService, int id) =>
{
    try
    {
        var task = await taskService.GetTaskByIdAsync(id);
        return Results.Ok(task);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (DbException ex)
    {
        return Results.Problem(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapPut("/api/tasks/{id:int}", async (ITaskService taskService, int id) =>
{
    try
    {
        var task = await taskService.UpdateTaskAsync(id);
        return Results.Ok(task);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (DbUpdateException ex)
    {
        return Results.Problem(ex.Message);
    }
    catch(InvalidOperationException ex)
    {
        return Results.Conflict(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();