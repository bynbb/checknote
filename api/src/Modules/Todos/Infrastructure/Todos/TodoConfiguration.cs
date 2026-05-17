namespace Checknote.Modules.Todos.Infrastructure.Todos;

using Checknote.Modules.Todos.Domain.Todos;
using Checknote.Modules.Todos.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class TodoConfiguration : IEntityTypeConfiguration<Todo>
{
    public void Configure(EntityTypeBuilder<Todo> builder)
    {
        builder.ToTable("TaskList", Schemas.Todos);

        builder.HasKey(todo => todo.Id);

        builder.Property(todo => todo.Id)
            .HasColumnType("bigint")
            .ValueGeneratedNever();

        builder.Property(todo => todo.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(todo => todo.IsCompleted)
            .HasColumnName("Completed")
            .IsRequired();

        builder.Property(todo => todo.Embedding)
            .HasColumnType("vector(3)");
    }
}
