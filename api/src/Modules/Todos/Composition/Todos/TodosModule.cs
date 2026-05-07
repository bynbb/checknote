namespace Checknote.Modules.Todos.Composition.Todos;

using System;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Application.Todos.SaveTaskList;
using Checknote.Modules.Todos.Infrastructure.Database;
using Checknote.Modules.Todos.Infrastructure.Todos;
using Checknote.Modules.Todos.Presentation.Todos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public static class TodosModule
{
    private const string ConnectionStringVariable = "CHECKNOTE_DATABASE_CONNECTION_STRING";

    public static IServiceCollection AddTodosModule(this IServiceCollection services)
    {
        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        }
        else
        {
            services.AddDbContext<TodosDbContext>(options => options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Todos)));
            services.AddScoped<ITodoRepository, SqlTodoRepository>();
        }

        services.AddScoped<GetTodosQueryHandler>();
        services.AddScoped<SaveTaskListCommandHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapTodosModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetTodosEndpoint();
        endpoints.MapSaveTaskListEndpoint();

        return endpoints;
    }
}
