using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheShop.Api.Middleware;

public class OpenApiSecurityMiddleware
{
    private readonly RequestDelegate _next;
    private const string OpenApiPath = "/openapi/v1.json";

    public OpenApiSecurityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.Equals(OpenApiPath))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        if (context.Response.StatusCode != StatusCodes.Status200OK)
        {
            buffer.Position = 0;
            await buffer.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
            return;
        }

        buffer.Position = 0;
        using var reader = new StreamReader(buffer);
        var json = await reader.ReadToEndAsync();

        var root = JsonNode.Parse(json) as JsonObject;
        if (root is null)
        {
            context.Response.Body = originalBody;
            await context.Response.WriteAsync(json);
            return;
        }

        InjectSecurityScheme(root);

        var output = root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

        context.Response.Body = originalBody;
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers.ContentLength = null;
        await context.Response.WriteAsync(output);
    }

    private static void InjectSecurityScheme(JsonObject root)
    {
        var components = (JsonObject?)root["components"] ?? new JsonObject();
        root["components"] = components;

        var securitySchemes = (JsonObject?)components["securitySchemes"] ?? new JsonObject();
        components["securitySchemes"] = securitySchemes;

        securitySchemes["Bearer"] = new JsonObject
        {
            ["type"] = "http",
            ["scheme"] = "bearer",
            ["bearerFormat"] = "JWT",
            ["description"] = "JWT Authorization header using the Bearer scheme."
        };

        root["security"] = new JsonArray(new JsonObject { ["Bearer"] = new JsonArray() });
    }
}

