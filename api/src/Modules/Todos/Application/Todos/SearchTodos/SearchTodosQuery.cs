namespace Checknote.Modules.Todos.Application.Todos.SearchTodos;

using System.Collections.Generic;
using Checknote.Common.Application.Messaging;
using Checknote.Modules.Todos.Domain.Todos;

public sealed record SearchTodosQuery(string? SearchText) : IQuery<IReadOnlyCollection<Todo>>;
