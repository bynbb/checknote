namespace Checknote.Modules.Todos.Domain.Todos;

using Checknote.Common.Domain;

public sealed class Todo : Entity<long>
{
    private Todo()
    {
        Title = string.Empty;
    }

    public Todo(long id, string title, bool isCompleted)
        : base(id)
    {
        Title = title;
        IsCompleted = isCompleted;
    }

    public string Title { get; private set; }

    public bool IsCompleted { get; private set; }
}
