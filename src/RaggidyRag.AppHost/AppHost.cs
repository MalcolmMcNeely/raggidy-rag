var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithImage("pgvector/pgvector", "pg17");
var store = postgres.AddDatabase("raggidyrag");

builder.AddProject<Projects.RaggidyRag_Api>("api")
    .WithReference(store)
    .WaitFor(store);

builder.Build().Run();
