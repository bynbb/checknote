namespace Checknote.Modules.Todos.Application.Todos.SearchTodos;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Application.Messaging;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class SearchTodosQueryHandler : IQueryHandler<SearchTodosQuery, IReadOnlyCollection<Todo>>
{
    private readonly ITodoRepository todoRepository;

    public SearchTodosQueryHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public Task<Result<IReadOnlyCollection<Todo>>> Handle(
        SearchTodosQuery query,
        CancellationToken cancellationToken)
    {
        string searchText = query.SearchText?.Trim() ?? string.Empty;

        if (searchText.Length == 0)
        {
            return Task.FromResult(Result.Success<IReadOnlyCollection<Todo>>([]));
        }

        return Task.FromResult(Result.Success(todoRepository.SearchTodos(searchText)));
    }
}
