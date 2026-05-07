namespace Checknote.Modules.Todos.Infrastructure.Database.Migrations;

using Checknote.Modules.Todos.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

[DbContext(typeof(TodosDbContext))]
[Migration("20260507000200_InitialTaskListSchema")]
public sealed partial class InitialTaskListSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(Schemas.Todos);

        migrationBuilder.CreateTable(
            name: "TaskList",
            schema: Schemas.Todos,
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false),
                Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Completed = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TaskList", task => task.Id);
            });

        migrationBuilder.Sql("""
            INSERT INTO [todos].[TaskList] ([Id], [Title], [Completed])
            VALUES (1, N'Build the app with Bazel', CAST(0 AS bit));
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "TaskList",
            schema: Schemas.Todos);
    }
}
