namespace Checknote.Api;

using Checknote.Api.Middleware;
using Checknote.Modules.Todos.Composition.Todos;
using Checknote.Modules.Users.Composition.Users;
using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public static class ChecknoteApi
{
    public const string HelloWorldRoute = "/hello-world";
    public const string HelloWorldResponse = "Hello from Checknote API";

    public static WebApplication Create(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        return Build(builder);
    }

    public static WebApplication Build(WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();
        builder.Services.AddTodosModule();
        builder.Services.AddUsersModule();

        WebApplication app = builder.Build();

        app.UseExceptionHandler();
        MapApiRoutes(app);
        app.MapTodosModule();
        app.MapUsersModule();
        MapStaticSite(app);

        return app;
    }

    private static void MapApiRoutes(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            PathString path = context.Request.Path;

            if (path == HelloWorldRoute || path == $"{HelloWorldRoute}/")
            {
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync(HelloWorldResponse);
                return;
            }

            if (path == "/health")
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("""{"service":"checknote-api","status":"ok"}""");
                return;
            }

            await next();
        });
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
