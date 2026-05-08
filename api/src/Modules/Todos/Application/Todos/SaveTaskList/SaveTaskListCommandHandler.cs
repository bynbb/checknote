namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Application.Messaging;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;

public sealed class SaveTaskListCommandHandler : ICommandHandler<SaveTaskListCommand>
{
    private readonly ITodoRepository todoRepository;

    public SaveTaskListCommandHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public Task<Result> Handle(SaveTaskListCommand command, CancellationToken cancellationToken)
    {
        if (command.Todos is null)
        {
            return Task.FromResult(Result.Failure(new ValidationError(
                [Error.Validation("Todos.TaskListRequired", "The task list is required.")])));
        }

        Result<Todo>[] todoResults = command.Todos
            .Select(todo => Todo.Create(todo.Id, todo.Title!.Trim(), todo.Completed))
            .ToArray();

        ValidationError validationError = ValidationError.FromResults(todoResults);
        if (validationError.Errors.Length > 0)
        {
            return Task.FromResult(Result.Failure(validationError));
        }

        Todo[] todos = todoResults.Select(result => result.Value).ToArray();

        todoRepository.SaveTodos(todos);
        return Task.FromResult(Result.Success());
    }
}
