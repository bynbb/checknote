namespace Checknote.Modules.Todos.Domain.Todos;

using Checknote.Common.Domain;

public sealed class Todo : Entity<string>
{
    public Todo(string id, string title, bool isCompleted)
        : base(id)
    {
        Title = title;
        IsCompleted = isCompleted;
    }

    public string Title { get; }

    public bool IsCompleted { get; }
}
