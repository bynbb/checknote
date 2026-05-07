namespace Checknote.Modules.Todos.Application.Todos.GetTodos;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Application.Messaging;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class GetTodosQueryHandler : IQueryHandler<GetTodosQuery, IReadOnlyCollection<Todo>>
{
    private readonly ITodoRepository todoRepository;

    public GetTodosQueryHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public Task<Result<IReadOnlyCollection<Todo>>> Handle(
        GetTodosQuery query,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(todoRepository.GetTodos()));
    }
}
