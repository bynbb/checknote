using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Checknote.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

int port = GetAvailablePort();
string baseAddress = $"http://127.0.0.1:{port}";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(baseAddress);

await using WebApplication app = ChecknoteApi.Build(builder);

await app.StartAsync();

try
{
    using var client = new HttpClient
    {
        BaseAddress = new Uri(baseAddress),
    };

    using HttpResponseMessage response = await client.GetAsync(ChecknoteApi.HelloWorldRoute);
    string body = await response.Content.ReadAsStringAsync();

    if (response.StatusCode != HttpStatusCode.OK)
    {
        Console.Error.WriteLine($"Expected 200 OK but received {(int)response.StatusCode} {response.StatusCode}.");
        return 1;
    }

    if (body.Trim() != ChecknoteApi.HelloWorldResponse)
    {
        Console.Error.WriteLine($"Expected '{ChecknoteApi.HelloWorldResponse}' but received '{body.Trim()}'.");
        return 1;
    }

    return 0;
}
finally
{
    await app.StopAsync();
}

static int GetAvailablePort()
{
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    int port = ((IPEndPoint)listener.LocalEndpoint).Port;
    listener.Stop();

    return port;
}
