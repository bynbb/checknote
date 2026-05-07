namespace Checknote.Modules.Todos.Composition.Todos;

using System;
using Checknote.Common.Presentation.Endpoints;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Infrastructure.Database;
using Checknote.Modules.Todos.Infrastructure.Todos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TodosPresentation = Checknote.Modules.Todos.Presentation.AssemblyReference;

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

        return services;
    }

    public static IEndpointRouteBuilder MapTodosModule(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapEndpoints(TodosPresentation.Assembly);
    }
}
