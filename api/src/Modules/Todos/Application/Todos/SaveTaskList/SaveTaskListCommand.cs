namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using System.Collections.Generic;
using Checknote.Common.Application.Messaging;

public sealed record SaveTaskListCommand(IReadOnlyCollection<SaveTaskListTodo>? Todos) : ICommand;

public sealed record SaveTaskListTodo(long Id, string? Title, bool Completed);
