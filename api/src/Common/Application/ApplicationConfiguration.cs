namespace Checknote.Common.Application;

using System;
using System.Reflection;
using Checknote.Common.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static class ApplicationConfiguration
{
    public const string MediatRLicenseKeyVariable = "CHECKNOTE_MEDIATR_LICENSE_KEY";

    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        params Assembly[] moduleAssemblies)
    {
        services.AddLogging();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(moduleAssemblies);
            configuration.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));

            string? licenseKey = Environment.GetEnvironmentVariable(MediatRLicenseKeyVariable);
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                configuration.LicenseKey = licenseKey;
            }
        });
        services.AddValidatorsFromAssemblies(
            moduleAssemblies,
            ServiceLifetime.Transient,
            includeInternalTypes: true);

        return services;
    }
}
