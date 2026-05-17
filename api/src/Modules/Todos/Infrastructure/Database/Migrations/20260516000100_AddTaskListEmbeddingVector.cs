namespace Checknote.Modules.Todos.Infrastructure.Database.Migrations;

using Checknote.Modules.Todos.Infrastructure.Database;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

[DbContext(typeof(TodosDbContext))]
[Migration("20260516000100_AddTaskListEmbeddingVector")]
public sealed partial class AddTaskListEmbeddingVector : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<SqlVector<float>>(
            name: "Embedding",
            schema: Schemas.Todos,
            table: "TaskList",
            type: "vector(3)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Embedding",
            schema: Schemas.Todos,
            table: "TaskList");
    }
}
