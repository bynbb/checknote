namespace Checknote.Modules.Todos.Presentation.Todos;

using System.Collections.Generic;
using System.Linq;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Application.Todos.SaveTaskList;
using Checknote.Modules.Todos.Domain.Todos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public static class GetTodosEndpoint
{
    public const string Route = "/api/todos";

    public static IEndpointRouteBuilder MapGetTodosEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, (GetTodosQueryHandler handler) =>
            Results.Ok(handler.Handle(new GetTodosQuery()).Select(TodoResponse.From)));

        return endpoints;
    }

    public static IEndpointRouteBuilder MapSaveTaskListEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut($"{Route}/task-list", (SaveTaskListRequest request, SaveTaskListCommandHandler handler) =>
        {
            Todo[] todos = request.Todos
                .Select(todo => new Todo(todo.Id, todo.Title.Trim(), todo.Completed))
                .Where(todo => !string.IsNullOrWhiteSpace(todo.Title))
                .ToArray();

            handler.Handle(new SaveTaskListCommand(todos));

            return Results.NoContent();
        });

        return endpoints;
    }
}

public sealed record TodoResponse(long Id, string Title, bool Completed)
{
    public static TodoResponse From(Todo todo)
    {
        return new TodoResponse(todo.Id, todo.Title, todo.IsCompleted);
    }
}

public sealed record SaveTaskListRequest(IReadOnlyCollection<TodoRequest> Todos);

public sealed record TodoRequest(long Id, string Title, bool Completed);
