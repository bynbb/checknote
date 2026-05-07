namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using System.Threading;
using System.Threading.Tasks;
using Checknote.Common.Application.Messaging;
using Checknote.Common.Domain;
using Checknote.Modules.Todos.Application.Abstractions;

public sealed class SaveTaskListCommandHandler : ICommandHandler<SaveTaskListCommand>
{
    private readonly ITodoRepository todoRepository;

    public SaveTaskListCommandHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public Task<Result> Handle(SaveTaskListCommand command, CancellationToken cancellationToken)
    {
        todoRepository.SaveTodos(command.Todos);
        return Task.FromResult(Result.Success());
    }
}
