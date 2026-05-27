var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ApiService>("apiservice")
    .WithExternalHttpEndpoints();

var web = builder.AddViteApp("webapp", "../WebApp")
    .WithExternalHttpEndpoints()
    .WithReference(api)
    .WithEnvironment("VITE_API_BASE", "https://localhost:5308");

builder.Build().Run();
