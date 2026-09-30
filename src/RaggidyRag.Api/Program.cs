using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaggidyRag.Api.Asking;
using RaggidyRag.Api.Chunking;
using RaggidyRag.Api.Chunks;
using RaggidyRag.Api.Embedding;
using RaggidyRag.Api.Ingesting;
using RaggidyRag.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

#pragma warning disable RS0030
builder.Services.TryAddSingleton(TimeProvider.System);
#pragma warning restore RS0030

var connectionString = builder.Configuration.GetConnectionString("raggidyrag")
    ?? throw new InvalidOperationException("The connection string raggidyrag is not set.");
builder.Services.AddPostgresVectorStore(connectionString);
builder.Services.AddSingleton<ChunkStore>();
builder.Services.AddSingleton<Chunker>();

builder.Services.AddOptions<VoyageOptions>().BindConfiguration("Voyage");
builder.Services.AddOptions<IngestOptions>().BindConfiguration("Ingest");

// The default resilience handler has timeouts of its own, and the Clock's wait on Voyage must be the only one.
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>, VoyageEmbeddingGenerator>()
    .RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

builder.Services.AddScoped<Ingest>();
builder.Services.AddSingleton<Ask>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPost("/ingest", (Ingest ingest, CancellationToken cancellation) => ingest.Run(cancellation));
app.MapPost("/ask", (Question question, Ask ask, CancellationToken cancellation) => ask.Answer(question, cancellation));

await app.Services.GetRequiredService<ChunkStore>().EnsureExists();

await app.RunAsync();
