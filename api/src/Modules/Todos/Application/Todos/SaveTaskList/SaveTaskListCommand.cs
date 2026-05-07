namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using System.Collections.Generic;
using Checknote.Common.Application.Messaging;
using Checknote.Modules.Todos.Domain.Todos;

public sealed record SaveTaskListCommand(IReadOnlyCollection<Todo> Todos) : ICommand;
