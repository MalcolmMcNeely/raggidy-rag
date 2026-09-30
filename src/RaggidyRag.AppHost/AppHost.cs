var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImage("pgvector/pgvector", "pg17");
var store = postgres.AddDatabase("raggidyrag");

var api = builder.AddProject<Projects.RaggidyRag_Api>("api")
    .WithReference(store)
    .WaitFor(store);

builder.AddViteApp("web", "../RaggidyRag.Web")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
