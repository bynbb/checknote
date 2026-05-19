namespace Checknote.Modules.Todos.Application.Abstractions;

using System.Collections.Generic;
using Checknote.Modules.Todos.Domain.Todos;

public interface ITodoRepository
{
    IReadOnlyCollection<Todo> GetTodos();

    IReadOnlyCollection<Todo> SearchTodos(string searchText);

    void SaveTodos(IReadOnlyCollection<Todo> todos);
}
