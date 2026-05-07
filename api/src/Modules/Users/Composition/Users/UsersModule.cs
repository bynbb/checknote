namespace Checknote.Modules.Users.Composition.Users;

using Checknote.Common.Presentation.Endpoints;
using Checknote.Modules.Users.Application.Abstractions;
using Checknote.Modules.Users.Infrastructure.Users;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UsersPresentation = Checknote.Modules.Users.Presentation.AssemblyReference;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        return services;
    }

    public static IEndpointRouteBuilder MapUsersModule(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapEndpoints(UsersPresentation.Assembly);
    }
}
