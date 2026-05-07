namespace Checknote.Modules.Todos.Application.Todos.GetTodos;

using System.Collections.Generic;
using Checknote.Common.Application.Messaging;
using Checknote.Modules.Todos.Domain.Todos;

public sealed record GetTodosQuery : IQuery<IReadOnlyCollection<Todo>>;
