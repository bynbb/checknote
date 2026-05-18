namespace Checknote.Modules.Todos.Domain.Todos;

using System;
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
        Embedding = CreateEmbedding(title);
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

    private static SqlVector<float> CreateEmbedding(string title)
    {
        string normalizedTitle = title.Trim().ToUpperInvariant();

        uint hash = 2166136261;
        int characterTotal = 0;

        foreach (char character in normalizedTitle)
        {
            characterTotal += character;
            hash ^= character;
            hash *= 16777619;
        }

        float lengthSignal = Math.Min(normalizedTitle.Length, 200) / 200f;
        float characterSignal = normalizedTitle.Length == 0
            ? 0f
            : characterTotal / (normalizedTitle.Length * 65535f);
        float hashSignal = (hash % 1000) / 1000f;

        return new SqlVector<float>(new ReadOnlyMemory<float>([
            lengthSignal,
            characterSignal,
            hashSignal,
        ]));
    }
}
