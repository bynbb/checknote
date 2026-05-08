namespace Checknote.Api.UnitTests.Modules.Database;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Checknote.Api.UnitTests.Support;
using Microsoft.EntityFrameworkCore;
using TodosDbContext = Checknote.Modules.Todos.Infrastructure.Database.TodosDbContext;
using TodosSchemas = Checknote.Modules.Todos.Infrastructure.Database.Schemas;
using UsersDbContext = Checknote.Modules.Users.Infrastructure.Database.UsersDbContext;
using UsersSchemas = Checknote.Modules.Users.Infrastructure.Database.Schemas;

internal static class SchemaBoundaryTests
{
    public static Task Run()
    {
        VerifyModuleSchema(
            "Users",
            "users",
            UsersSchemas.Users,
            new UsersDbContext(
                CreateOptions<UsersDbContext>("UsersSchemaBoundary")));

        VerifyModuleSchema(
            "Todos",
            "todos",
            TodosSchemas.Todos,
            new TodosDbContext(
                CreateOptions<TodosDbContext>("TodosSchemaBoundary")));

        return Task.CompletedTask;
    }

    private static DbContextOptions<TContext> CreateOptions<TContext>(string databaseName)
        where TContext : DbContext
    {
        string connectionString =
            $"Server=(local);Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True";

        return new DbContextOptionsBuilder<TContext>()
            .UseSqlServer(connectionString)
            .Options;
    }

    private static void VerifyModuleSchema(
        string moduleName,
        string expectedSchema,
        string actualSchemaConstant,
        DbContext dbContext)
    {
        using (dbContext)
        {
            TestAssert.Equal(
                expectedSchema,
                actualSchemaConstant,
                $"{moduleName} schema constant should match lowercase module identity.");

            TestAssert.Equal(
                expectedSchema,
                dbContext.Model.GetDefaultSchema(),
                $"{moduleName} DbContext default schema");

            IReadOnlyCollection<string> mappedSchemas = dbContext.Model
                .GetEntityTypes()
                .Where(entityType => !string.IsNullOrWhiteSpace(entityType.GetTableName()))
                .Select(entityType => entityType.GetSchema() ?? dbContext.Model.GetDefaultSchema() ?? string.Empty)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            TestAssert.True(
                mappedSchemas.Count > 0,
                $"{moduleName} should have at least one mapped table.");

            foreach (string mappedSchema in mappedSchemas)
            {
                TestAssert.Equal(
                    expectedSchema,
                    mappedSchema,
                    $"{moduleName} mapped table schema");
            }
        }
    }
}
