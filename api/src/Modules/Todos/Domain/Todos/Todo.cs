namespace Checknote.Modules.Todos.Domain.Todos;

using Checknote.Common.Domain;
using Microsoft.Data.SqlTypes;

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

    public SqlVector<float>? Embedding { get; private set; }

    public static Result<Todo> Create(long id, string title, bool isCompleted)
    {
        if (title == "88888")
        {
            return Result.Failure<Todo>(TodoErrors.ReservedTitle);
        }

        return new Todo(id, title, isCompleted);
    }
}
