using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Design;
using TodoApi;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

// Configure the database context
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("tododb"), new MySqlServerVersion(new Version(8, 0, 21))));

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
        });
});

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use CORS policy
app.UseCors("AllowAllOrigins");

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Route to get all tasks
app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    return await context.Tasks.ToListAsync();
});

// Roate to egt task by id
app.MapGet("/tasks/{id}", async (HttpContext httpContext, ToDoDbContext context) =>
{
    if (!int.TryParse(httpContext.Request.RouteValues["id"]?.ToString(), out var id))
    {
        return Results.BadRequest();
    }

    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();
    return Results.Ok(task);
});

// Route to add a new task
app.MapPost("/tasks", async (ToDoDbContext context, TodoApi.Task task) =>
{
    context.Tasks.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// Route to update a task
app.MapPut("/tasks/{id}", async (int id, TodoApi.Task updatedTask, ToDoDbContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Name = updatedTask.Name; // Assuming there is a field named Name
    task.IsComplete = updatedTask.IsComplete; // Assuming there is a field named IsComplete
    context.Tasks.Update(task); // Ensure the task is marked as updated
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Route to delete a task
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    context.Tasks.Remove(task);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();