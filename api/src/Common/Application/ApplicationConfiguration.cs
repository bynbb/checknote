namespace Checknote.Common.Application;

using System;
using System.Reflection;
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

            string? licenseKey = Environment.GetEnvironmentVariable(MediatRLicenseKeyVariable);
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                configuration.LicenseKey = licenseKey;
            }
        });

        return services;
    }
}
