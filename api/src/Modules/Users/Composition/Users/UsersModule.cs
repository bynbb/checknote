namespace Checknote.Modules.Users.Composition.Users;

using System;
using Checknote.Common.Presentation.Endpoints;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Infrastructure.Database;
using Checknote.Modules.Users.Infrastructure.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using UsersPresentation = Checknote.Modules.Users.Presentation.AssemblyReference;

public static class UsersModule
{
    private const string ConnectionStringVariable = "CHECKNOTE_DATABASE_CONNECTION_STRING";

    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddEndpoints(UsersPresentation.Assembly);
        services.AddScoped<ICurrentUserProvider, AuthenticatedCurrentUserProvider>();

        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        }
        else
        {
            services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Users)));
            services.AddScoped<IUserRepository, SqlUserRepository>();
        }

        return services;
    }
}
