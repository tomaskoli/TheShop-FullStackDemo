using System.Text;
using System.Threading.RateLimiting;
using Basket.Application.Commands;
using Catalog.Application.Commands;
using Chatbot.Application.Commands;
using Chatbot.Infrastructure;
using FluentValidation;
using Identity.Application.Commands;
using Identity.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Ordering.Application.Commands;
using TheShop.Api.Auth;
using TheShop.Api.Endpoints;
using TheShop.Api.Middleware;
using TheShop.Infrastructure;
using TheShop.Infrastructure.Settings;
using TheShop.Messaging;
using TheShop.ServiceDefaults;
using TheShop.SharedKernel.Behaviors;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5001", "https://localhost:5001"];
        
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    options.AddPolicy("Strict", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5001", "https://localhost:5001"];
        
        policy
            .WithOrigins(allowedOrigins)
            .WithMethods("POST")
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    options.AddFixedWindowLimiter("Global", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 10;
    });
    
    options.AddSlidingWindowLimiter("Auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(5);
        limiterOptions.SegmentsPerWindow = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
    
    options.AddSlidingWindowLimiter("Login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(15);
        limiterOptions.SegmentsPerWindow = 5;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";
        
        var problem = new
        {
            type = "https://tools.ietf.org/html/rfc6585#section-4",
            title = "Too Many Requests",
            status = 429,
            detail = "Rate limit exceeded. Please try again later.",
            instance = context.HttpContext.Request.Path.Value
        };
        
        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
        
        options.EventsType = typeof(SessionValidationHandler);
    });

builder.Services.AddScoped<SessionValidationHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireRole(UserRole.Admin.ToString()));

    options.AddPolicy(Policies.CustomerOrAdmin, policy =>
        policy.RequireRole(UserRole.Customer.ToString(), UserRole.Admin.ToString()));
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserCommand>();
    cfg.RegisterServicesFromAssemblyContaining<CreateProductCommand>();
    cfg.RegisterServicesFromAssemblyContaining<AddToBasketCommand>();
    cfg.RegisterServicesFromAssemblyContaining<CreateOrderCommand>();
    cfg.RegisterServicesFromAssemblyContaining<ChatCommand>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<AddToBasketCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<ChatCommand>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddChatbot(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseCors("Default");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<OpenApiSecurityMiddleware>();
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Open API v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();

app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapBasketEndpoints();
app.MapOrderingEndpoints();
app.MapChatbotEndpoints();

app.Run();

public partial class Program { }
