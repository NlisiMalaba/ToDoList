using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ToDoList.Application.TodoTasks;
using ToDoList.Infrastructure;
using ToDoList.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ToDoList API",
        Version = "v1",
        Description = "Simple Clean Architecture To-Do list Web API for learning CI/CD with Jenkins."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Server=localhost;Port=3306;Database=todo_db;User=todo_user;Password=todo_password;";

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<CreateTodoTaskService>();
builder.Services.AddScoped<GetTodoTaskService>();
builder.Services.AddScoped<ListTodoTasksService>();
builder.Services.AddScoped<UpdateTodoTaskService>();
builder.Services.AddScoped<DeleteTodoTaskService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var applyMigrationsOnStartup = builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");
    if (applyMigrationsOnStartup)
    {
        db.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

