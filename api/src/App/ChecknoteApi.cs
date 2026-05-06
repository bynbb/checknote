namespace Checknote.Api;

using Checknote.Modules.Todos.Composition.Todos;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
        builder.Services.AddTodosModule();

        WebApplication app = builder.Build();

        MapApiRoutes(app);
        app.MapTodosModule();
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
        app.UseStaticFiles();

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
            await context.Response.SendFileAsync(indexPath);
        });
    }
}
