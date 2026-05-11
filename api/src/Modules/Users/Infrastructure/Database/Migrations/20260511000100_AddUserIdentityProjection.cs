namespace Checknote.Modules.Users.Infrastructure.Database.Migrations;

using System;
using Checknote.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

[DbContext(typeof(UsersDbContext))]
[Migration("20260511000100_AddUserIdentityProjection")]
public sealed partial class AddUserIdentityProjection : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Users",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.AddColumn<Guid>(
            name: "ChecknoteUserId",
            schema: Schemas.Users,
            table: "Users",
            type: "uniqueidentifier",
            nullable: false,
            defaultValueSql: "NEWID()");

        migrationBuilder.AddColumn<string>(
            name: "IdentityId",
            schema: Schemas.Users,
            table: "Users",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE [users].[Users]
            SET [IdentityId] = [Id]
            WHERE [IdentityId] IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "IdentityId",
            schema: Schemas.Users,
            table: "Users",
            type: "nvarchar(128)",
            maxLength: 128,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(128)",
            oldMaxLength: 128,
            oldNullable: true);

        migrationBuilder.DropColumn(
            name: "Id",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "ChecknoteUserId",
            schema: Schemas.Users,
            table: "Users",
            newName: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Users",
            schema: Schemas.Users,
            table: "Users",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            schema: Schemas.Users,
            table: "Users",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_IdentityId",
            schema: Schemas.Users,
            table: "Users",
            column: "IdentityId",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Users_Email",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_IdentityId",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.DropPrimaryKey(
            name: "PK_Users",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.AddColumn<string>(
            name: "LegacyUserId",
            schema: Schemas.Users,
            table: "Users",
            type: "nvarchar(64)",
            maxLength: 64,
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.Sql("""
            UPDATE [users].[Users]
            SET [LegacyUserId] = LEFT([IdentityId], 64);
            """);

        migrationBuilder.DropColumn(
            name: "Id",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.DropColumn(
            name: "IdentityId",
            schema: Schemas.Users,
            table: "Users");

        migrationBuilder.RenameColumn(
            name: "LegacyUserId",
            schema: Schemas.Users,
            table: "Users",
            newName: "Id");

        migrationBuilder.AddPrimaryKey(
            name: "PK_Users",
            schema: Schemas.Users,
            table: "Users",
            column: "Id");

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            schema: Schemas.Users,
            table: "Users",
            column: "Email",
            unique: true);
    }
}
