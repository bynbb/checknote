namespace Checknote.Modules.Users.Infrastructure.Database.Migrations;

using Checknote.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

[DbContext(typeof(UsersDbContext))]
[Migration("20260507000100_InitialUsersSchema")]
public sealed partial class InitialUsersSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(Schemas.Users);

        migrationBuilder.CreateTable(
            name: "Users",
            schema: Schemas.Users,
            columns: table => new
            {
                Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", user => user.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            schema: Schemas.Users,
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.Sql("""
            INSERT INTO [users].[Users] ([Id], [Name], [Email])
            VALUES (N'user-1', N'Ada Lovelace', N'ada@checknote.local');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users",
            schema: Schemas.Users);
    }
}
