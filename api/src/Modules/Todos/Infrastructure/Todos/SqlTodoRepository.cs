namespace Checknote.Modules.Todos.Infrastructure.Todos;

using System.Collections.Generic;
using System.Linq;
using Checknote.Modules.Todos.Application.Abstractions;
using Checknote.Modules.Todos.Domain.Todos;
using Checknote.Modules.Todos.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

public sealed class SqlTodoRepository : ITodoRepository
{
    private readonly TodosDbContext dbContext;

    public SqlTodoRepository(TodosDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IReadOnlyCollection<Todo> GetTodos()
    {
        return dbContext.TaskList
            .AsNoTracking()
            .OrderByDescending(todo => todo.Id)
            .ToArray();
    }

    public void SaveTodos(IReadOnlyCollection<Todo> todos)
    {
        Todo[] existingTodos = dbContext.TaskList.ToArray();
        dbContext.TaskList.RemoveRange(existingTodos);
        dbContext.TaskList.AddRange(todos);
        dbContext.SaveChanges();
    }
}
