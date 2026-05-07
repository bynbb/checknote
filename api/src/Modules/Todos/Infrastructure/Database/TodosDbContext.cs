namespace Checknote.Modules.Todos.Infrastructure.Database;

using Checknote.Modules.Todos.Domain.Todos;
using Checknote.Modules.Todos.Infrastructure.Todos;
using Microsoft.EntityFrameworkCore;

public sealed class TodosDbContext : DbContext
{
    public TodosDbContext(DbContextOptions<TodosDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> TaskList => Set<Todo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Todos);
        modelBuilder.ApplyConfiguration(new TodoConfiguration());
    }
}
