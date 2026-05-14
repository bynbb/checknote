namespace Checknote.Api;

using Checknote.Api.Authentication;
using Checknote.Api.Logging;
using Checknote.Api.Middleware;
using Checknote.Common.Application;
using Checknote.Common.Presentation.Endpoints;
using Checknote.Modules.Todos.Composition.Todos;
using Checknote.Modules.Users.Composition.Users;
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using TodosApplication = Checknote.Modules.Todos.Application.AssemblyReference;
using UsersApplication = Checknote.Modules.Users.Application.AssemblyReference;

public static class ChecknoteApi
{
    public const string HelloWorldRoute = "/hello-world";
    public const string HelloWorldResponse = "Hello from Checknote API";
    public const string TestNotFoundRoute = "/api/test-errors/throw-404";
    public const string EnableSwaggerVariable = "CHECKNOTE_ENABLE_SWAGGER";
    private const string MediatRLicenseLogCategory = "LuckyPennySoftware.MediatR.License";

    public static WebApplication Create(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        return Build(builder);
    }

    public static WebApplication Build(WebApplicationBuilder builder)
    {
        ConfigureSerilog(builder);
        builder.Logging.AddFilter(MediatRLicenseLogCategory, LogLevel.None);
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddChecknoteAuthentication(builder.Configuration);
        builder.Services.AddApplication(
            TodosApplication.Assembly,
            UsersApplication.Assembly);
        builder.Services.AddTodosModule();
        builder.Services.AddUsersModule();

        WebApplication app = builder.Build();

        if (ShouldExposeSwagger(app))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler();
        app.UseStatusCodePages(async statusCodeContext =>
        {
            HttpContext httpContext = statusCodeContext.HttpContext;

            if (!ErrorResponsePolicy.ShouldServeFriendlyErrorPage(httpContext.Request))
            {
                return;
            }

            string errorPagePath = ErrorResponsePolicy.GetErrorPagePath(app.Environment);

            if (!File.Exists(errorPagePath))
            {
                return;
            }

            httpContext.Response.ContentType = "text/html; charset=utf-8";
            httpContext.Response.Headers.CacheControl = ErrorResponsePolicy.NoCacheHeader;
            httpContext.Response.Headers.Pragma = "no-cache";
            httpContext.Response.Headers.Expires = "0";

            await httpContext.Response.SendFileAsync(errorPagePath);
        });
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = SerilogRequestLogPolicy.MessageTemplate;
            options.GetLevel = (httpContext, elapsed, exception) =>
                ToSerilogLevel(SerilogRequestLogPolicy.GetLevel(httpContext, elapsed, exception));
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? string.Empty);
                diagnosticContext.Set("ChecknoteArea", SerilogRequestLogPolicy.GetArea(httpContext.Request.Path));
            };
        });
        MapErrorPageAssets(app);
        app.UseAuthentication();
        app.UseAuthorization();
        MapApiRoutes(app);
        app.MapEndpoints();
        MapStaticSite(app);

        return app;
    }

    private static LogEventLevel ToSerilogLevel(ChecknoteRequestLogLevel level) =>
        level switch
        {
            ChecknoteRequestLogLevel.Error => LogEventLevel.Error,
            ChecknoteRequestLogLevel.Warning => LogEventLevel.Warning,
            _ => LogEventLevel.Information,
        };

    private static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(context.Configuration);

            string? seqServerUrl = Environment.GetEnvironmentVariable(
                SerilogRequestLogPolicy.SeqServerUrlVariable);

            if (!string.IsNullOrWhiteSpace(seqServerUrl))
            {
                loggerConfiguration.WriteTo.Seq(
                    seqServerUrl,
                    apiKey: Environment.GetEnvironmentVariable(
                        SerilogRequestLogPolicy.SeqApiKeyVariable));
            }
        });
    }

    private static void MapApiRoutes(WebApplication app)
    {
        app.MapGet(
                HelloWorldRoute,
                () => Results.Text(HelloWorldResponse, "text/plain"))
            .WithName("HelloWorld")
            .WithTags("System")
            .Produces<string>(StatusCodes.Status200OK, "text/plain");

        app.MapGet(
                TestNotFoundRoute,
                () => Results.NotFound())
            .WithName("ThrowTest404")
            .WithTags("System")
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet(
                "/health",
                () => Results.Ok(new HealthResponse("checknote-api", "ok")))
            .WithName("Health")
            .WithTags("System")
            .Produces<HealthResponse>(StatusCodes.Status200OK);

        app.MapGet(
                "/api/auth/config",
                (IOptions<ChecknoteKeycloakOptions> options) =>
                    Results.Ok(AuthConfigResponse.From(options.Value)))
            .WithName("GetAuthConfig")
            .WithTags("Auth")
            .Produces<AuthConfigResponse>(StatusCodes.Status200OK);
    }

    private static void MapStaticSite(WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
                ApplyStaticFileCachePolicy(context.Context.Response, context.File.Name),
        });

        app.Use(async (context, next) =>
        {
            await next();

            if (context.Response.HasStarted ||
                context.Response.StatusCode != StatusCodes.Status404NotFound ||
                context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                Path.HasExtension(context.Request.Path))
            {
                return;
            }

            string indexPath = Path.Combine(app.Environment.WebRootPath ?? string.Empty, "index.html");

            if (!File.Exists(indexPath))
            {
                return;
            }

            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "text/html";
            ApplyStaticFileCachePolicy(context.Response, "index.html");
            await context.Response.SendFileAsync(indexPath);
        });
    }

    private static void MapErrorPageAssets(WebApplication app)
    {
        string errorPagesPath = ErrorResponsePolicy.GetErrorPagesPath(app.Environment);

        if (!Directory.Exists(errorPagesPath))
        {
            return;
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(errorPagesPath),
            RequestPath = ErrorResponsePolicy.ErrorAssetsRequestPath,
            OnPrepareResponse = context =>
                ApplyStaticFileCachePolicy(context.Context.Response, context.File.Name),
        });
    }

    private static bool ShouldExposeSwagger(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            return true;
        }

        string? setting = Environment.GetEnvironmentVariable(EnableSwaggerVariable);

        return bool.TryParse(setting, out bool enabled) && enabled;
    }

    private static void ApplyStaticFileCachePolicy(HttpResponse response, string fileName)
    {
        if (string.Equals(fileName, "index.html", StringComparison.OrdinalIgnoreCase))
        {
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";
            return;
        }

        if (IsAngularFingerprintAsset(fileName))
        {
            response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
    }

    private static bool IsAngularFingerprintAsset(string fileName)
    {
        string extension = Path.GetExtension(fileName);

        if (!string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string name = Path.GetFileNameWithoutExtension(fileName);
        int hashSeparatorIndex = name.LastIndexOf('-');

        return hashSeparatorIndex >= 0 &&
            hashSeparatorIndex < name.Length - 1 &&
            IsAlphaNumeric(name[(hashSeparatorIndex + 1)..]);
    }

    private static bool IsAlphaNumeric(string value)
    {
        foreach (char character in value)
        {
            if (!char.IsLetterOrDigit(character))
            {
                return false;
            }
        }

        return true;
    }
}

public sealed record HealthResponse(string Service, string Status);

public sealed record AuthConfigResponse(
    bool Enabled,
    string AuthServerUrl,
    string Realm,
    string ClientId)
{
    public static AuthConfigResponse From(ChecknoteKeycloakOptions options)
    {
        bool enabled =
            !string.IsNullOrWhiteSpace(options.AuthServerUrl) &&
            !string.IsNullOrWhiteSpace(options.Realm) &&
            !string.IsNullOrWhiteSpace(options.PublicClientId);

        return new AuthConfigResponse(
            enabled,
            options.AuthServerUrl,
            options.Realm,
            options.PublicClientId);
    }
}
