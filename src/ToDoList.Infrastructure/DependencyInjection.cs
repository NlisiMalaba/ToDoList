using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ToDoList.Domain.Repositories;
using ToDoList.Infrastructure.Data;
using ToDoList.Infrastructure.Repositories;

namespace ToDoList.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion);
        });

        services.AddScoped<ITodoTaskRepository, TodoTaskRepository>();

        return services;
    }
}

