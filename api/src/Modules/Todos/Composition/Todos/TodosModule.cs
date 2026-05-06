namespace Checknote.Modules.Todos.Composition.Todos;

using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Infrastructure.Todos;
using Checknote.Modules.Todos.Presentation.Todos;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public static class TodosModule
{
    public static IServiceCollection AddTodosModule(this IServiceCollection services)
    {
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        services.AddScoped<GetTodosQueryHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapTodosModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetTodosEndpoint();

        return endpoints;
    }
}
