namespace Checknote.Common.Presentation.Endpoints;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpoints);
}

public static class EndpointRegistrationExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, params Assembly[] assemblies)
    {
        ServiceDescriptor[] serviceDescriptors = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                typeof(IEndpoint).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }

    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
