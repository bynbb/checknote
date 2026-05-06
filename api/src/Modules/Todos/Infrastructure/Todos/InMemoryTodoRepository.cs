namespace Checknote.Modules.Todos.Infrastructure.Todos;

using System.Collections.Generic;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class InMemoryTodoRepository : ITodoRepository
{
    private static readonly Todo[] SeedTodos =
    [
        new Todo("todo-1", "Seed the Checknote API module", false),
    ];

    public IReadOnlyCollection<Todo> GetTodos()
    {
        return SeedTodos;
    }
}
