namespace Checknote.Modules.Todos.Application.Todos.SaveTaskList;

using Checknote.Modules.Todos.Application.Abstractions;

public sealed class SaveTaskListCommandHandler
{
    private readonly ITodoRepository todoRepository;

    public SaveTaskListCommandHandler(ITodoRepository todoRepository)
    {
        this.todoRepository = todoRepository;
    }

    public void Handle(SaveTaskListCommand command)
    {
        todoRepository.SaveTodos(command.Todos);
    }
}
