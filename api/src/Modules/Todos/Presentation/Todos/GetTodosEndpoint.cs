namespace Checknote.Modules.Todos.Presentation.Todos;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Domain;
using Checknote.Common.Presentation.Endpoints;
using Checknote.Common.Presentation.Results;
using Checknote.Modules.Todos.Application.Todos.GetTodos;
using Checknote.Modules.Todos.Application.Todos.SaveTaskList;
using Checknote.Modules.Todos.Domain.Todos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

public sealed class GetTodosEndpoint : IEndpoint
{
    public const string Route = "/api/todos";

    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(Route, async (ISender sender, CancellationToken cancellationToken) =>
        {
            Result<IReadOnlyCollection<Todo>> result = await sender.Send(
                new GetTodosQuery(),
                cancellationToken);

            return result.Match<IReadOnlyCollection<Todo>, IResult>(
                todos => Results.Ok(todos.Select(TodoResponse.From)),
                ApiResults.Problem);
        })
        .WithName("GetTodos")
        .WithTags("Todos")
        .RequireAuthorization()
        .Produces<IEnumerable<TodoResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed class SaveTaskListEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            $"{GetTodosEndpoint.Route}/task-list",
            async (
                SaveTaskListRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                SaveTaskListTodo[]? todos = request.Todos?
                    .Select(todo => new SaveTaskListTodo(todo.Id, todo.Title, todo.Completed))
                    .ToArray();

                Result result = await sender.Send(
                    new SaveTaskListCommand(todos),
                    cancellationToken);

                return result.Match<IResult>(
                    Results.NoContent,
                    ApiResults.Problem);
            })
            .WithName("SaveTaskList")
            .WithTags("Todos")
            .RequireAuthorization()
            .Accepts<SaveTaskListRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed record TodoResponse(long Id, string Title, bool Completed)
{
    public static TodoResponse From(Todo todo)
    {
        return new TodoResponse(todo.Id, todo.Title, todo.IsCompleted);
    }
}

public sealed record SaveTaskListRequest(IReadOnlyCollection<TodoRequest>? Todos);

public sealed record TodoRequest(long Id, string? Title, bool Completed);
