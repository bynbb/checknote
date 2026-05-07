namespace Checknote.Modules.Todos.Application.Abstractions;

using System.Collections.Generic;
using Checknote.Modules.Todos.Domain.Todos;

public interface ITodoRepository
{
    IReadOnlyCollection<Todo> GetTodos();

    void SaveTodos(IReadOnlyCollection<Todo> todos);
}
