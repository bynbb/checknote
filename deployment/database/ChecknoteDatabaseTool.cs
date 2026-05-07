namespace Checknote.Deployment.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;
using TodosDbContext = Checknote.Modules.Todos.Infrastructure.Database.TodosDbContext;
using TodosSchemas = Checknote.Modules.Todos.Infrastructure.Database.Schemas;
using UsersDbContext = Checknote.Modules.Users.Infrastructure.Database.UsersDbContext;
using UsersSchemas = Checknote.Modules.Users.Infrastructure.Database.Schemas;

public static class ChecknoteDatabaseTool
{
    private const string ConnectionStringVariable = "CHECKNOTE_DATABASE_CONNECTION_STRING";
    private const string EnvironmentVariable = "CHECKNOTE_DEPLOYMENT_ENVIRONMENT";

    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        if (args.Length != 1 || !string.Equals(args[0], "apply", StringComparison.OrdinalIgnoreCase))
        {
            error.WriteLine("Usage: checknote_db_tool apply");
            return 2;
        }

        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            error.WriteLine($"{ConnectionStringVariable} must be set.");
            return 2;
        }

        string environment = Environment.GetEnvironmentVariable(EnvironmentVariable) ?? "local";

        DbContextOptions<UsersDbContext> usersOptions = new DbContextOptionsBuilder<UsersDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    UsersSchemas.Users))
            .Options;

        output.WriteLine($"Applying Users database model to {environment}.");
        using (UsersDbContext usersDbContext = new(usersOptions))
        {
            Migrate(usersDbContext, "Users");
        }
        output.WriteLine("Users database model applied.");

        DbContextOptions<TodosDbContext> todosOptions = new DbContextOptionsBuilder<TodosDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    TodosSchemas.Todos))
            .Options;

        output.WriteLine($"Applying Todos database model to {environment}.");
        using (TodosDbContext todosDbContext = new(todosOptions))
        {
            Migrate(todosDbContext, "Todos");
        }
        output.WriteLine("Todos database model applied.");

        return 0;
    }

    private static void Migrate(DbContext dbContext, string moduleName)
    {
        object migratorService = dbContext.GetInfrastructure().GetService(typeof(IMigrator))
            ?? throw new InvalidOperationException($"The {moduleName} migrator service was not available.");
        IMigrator migrator = (IMigrator)migratorService;

        migrator.Migrate();
    }
}
