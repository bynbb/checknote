namespace Checknote.Modules.Todos.Infrastructure.Todos;

using System.Collections.Generic;
using System.Linq;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class InMemoryTodoRepository : ITodoRepository
{
    private static readonly object Sync = new();
    private static List<Todo> todos =
    [
        new Todo(1L, "Build the app with Bazel", false),
    ];

    public IReadOnlyCollection<Todo> GetTodos()
    {
        lock (Sync)
        {
            return todos.ToArray();
        }
    }

    public IReadOnlyCollection<Todo> SearchTodos(string searchText)
    {
        lock (Sync)
        {
            return todos
                .Where(todo => todo.Title.Contains(searchText, System.StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }

    public void SaveTodos(IReadOnlyCollection<Todo> newTodos)
    {
        lock (Sync)
        {
            todos = newTodos.ToList();
        }
    }
}
