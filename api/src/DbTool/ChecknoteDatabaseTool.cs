namespace Checknote.DbTool;

using Checknote.Modules.Users.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;

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

        DbContextOptions<UsersDbContext> options = new DbContextOptionsBuilder<UsersDbContext>()
            .UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Users))
            .Options;

        using UsersDbContext usersDbContext = new(options);
        object usersMigratorService = usersDbContext.GetInfrastructure().GetService(typeof(IMigrator))
            ?? throw new InvalidOperationException("The Users migrator service was not available.");
        IMigrator usersMigrator = (IMigrator)usersMigratorService;

        output.WriteLine($"Applying Users database model to {environment}.");
        usersMigrator.Migrate();
        output.WriteLine("Users database model applied.");

        return 0;
    }
}
