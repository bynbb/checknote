namespace Checknote.Modules.Todos.Application.Todos.GetTodos;

using System.Collections.Generic;
using Checknote.Common.Application.Messaging;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class GetTodosQueryHandler : IQueryHandler<GetTodosQuery, IReadOnlyCollection<Todo>>
{
    private readonly ITodoRepository todoRepository;

    public GetTodosQueryHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public IReadOnlyCollection<Todo> Handle(GetTodosQuery query)
    {
        return todoRepository.GetTodos();
    }
}
