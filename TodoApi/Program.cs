using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Configure the database context
builder.Services.AddDbContext<TaskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("tododb")));

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
    c.RoutePrefix = string.Empty; // Set the Swagger UI at the app's root
});

// Route to get all tasks
app.MapGet("/tasks", async (TaskContext context) =>
{
    return await context.Tasks.ToListAsync();
});

// Route to add a new task
app.MapPost("/tasks", async (TaskContext context, TodoItem task) =>
{
    context.Tasks.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// Route to update a task
app.MapPut("/tasks/{id}", async (int id, TodoItem updatedTask, TaskContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    task.Name = updatedTask.Name; // Assuming there is a field named Name
    task.IsComplete = updatedTask.IsComplete; // Assuming there is a field named IsComplete
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// Route to delete a task
app.MapDelete("/tasks/{id}", async (int id, TaskContext context) =>
{
    var task = await context.Tasks.FindAsync(id);
    if (task is null) return Results.NotFound();

    context.Tasks.Remove(task);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// Models

public class TaskContext : DbContext
{
    public TaskContext(DbContextOptions<TaskContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> Tasks { get; set; }
}

public class TodoItem
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsComplete { get; set; }
}
