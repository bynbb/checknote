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
        });
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
                Result<Todo>[] todoResults = request.Todos
                    .Select(todo => new TodoRequest(todo.Id, todo.Title.Trim(), todo.Completed))
                    .Where(todo => !string.IsNullOrWhiteSpace(todo.Title))
                    .Select(todo => Todo.Create(todo.Id, todo.Title, todo.Completed))
                    .ToArray();

                ValidationError validationError = ValidationError.FromResults(todoResults);

                if (validationError.Errors.Length > 0)
                {
                    return ApiResults.Problem(Result.Failure(validationError));
                }

                Todo[] todos = todoResults.Select(result => result.Value).ToArray();

                Result result = await sender.Send(
                    new SaveTaskListCommand(todos),
                    cancellationToken);

                return result.Match<IResult>(
                    Results.NoContent,
                    ApiResults.Problem);
            });
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
