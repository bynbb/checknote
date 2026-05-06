namespace Checknote.Modules.Users.Composition.Users;

using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Application.Users.GetCurrentUser;
using Checknote.Modules.Users.Infrastructure.Users;
using Checknote.Modules.Users.Presentation.Users;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<GetCurrentUserQueryHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapUsersModule(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetCurrentUserEndpoint();

        return endpoints;
    }
}
