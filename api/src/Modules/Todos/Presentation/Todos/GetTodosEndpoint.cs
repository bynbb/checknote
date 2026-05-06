namespace Checknote.Modules.Todos.Presentation.Todos;

using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class GetTodosEndpoint
{
    public const string Route = "/api/todos";

    public static IEndpointRouteBuilder MapGetTodosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, (GetTodosQueryHandler handler) =>
            Results.Ok(handler.Handle(new GetTodosQuery())));

        return endpoints;
    }
}
