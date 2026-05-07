namespace Checknote.Common.Presentation.Endpoints;

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpoints);
}

public static class EndpointRegistrationExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder endpoints,
        params Assembly[] assemblies)
    {
        Type[] endpointTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                typeof(IEndpoint).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        foreach (Type endpointType in endpointTypes)
        {
            if (Activator.CreateInstance(endpointType, nonPublic: true) is not IEndpoint endpoint)
            {
                throw new InvalidOperationException(
                    $"Endpoint type {endpointType.FullName} could not be created.");
            }

            endpoint.MapEndpoint(endpoints);
        }

        return endpoints;
    }
}
