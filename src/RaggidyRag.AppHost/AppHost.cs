var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImage("pgvector/pgvector", "pg17");
var store = postgres.AddDatabase("raggidyrag");

// The keys live in the app host's user secrets as Parameters:voyage-key and Parameters:claude-key, and never in a settings file.
var voyageKey = builder.AddParameter("voyage-key", secret: true);
var claudeKey = builder.AddParameter("claude-key", secret: true);

var api = builder.AddProject<Projects.RaggidyRag_Api>("api")
    .WithReference(store)
    .WaitFor(store)
    .WithEnvironment("Voyage__ApiKey", voyageKey)
    .WithEnvironment("Claude__ApiKey", claudeKey);

builder.AddViteApp("web", "../RaggidyRag.Web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
