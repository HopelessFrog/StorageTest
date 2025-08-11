using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .AddDatabase("storage-db");

var storage = builder.AddProject<Storage>("storage")
    .WithReference(postgres)
    .WaitFor(postgres);

builder.AddNpmApp("frontend", "../frontend")
    .WithReference(storage)
    .WaitFor(storage)
    .WithEnvironment("BROWSER", "none")
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.Build().Run();