var builder = DistributedApplication.CreateBuilder(args);

// API service
var api = builder.AddProject<Projects.TheShop_Api>("api")
    .WithExternalHttpEndpoints();

// Blazor Web App
builder.AddProject<Projects.TheShop_WebApp>("webapp")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
