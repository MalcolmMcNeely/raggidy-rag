using Microsoft.Extensions.DependencyInjection.Extensions;
using RaggidyRag.Api.Asking;
using RaggidyRag.Api.Chunks;
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
builder.Services.AddSingleton<Ask>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapPost("/ask", (Question question, Ask ask, CancellationToken cancellation) => ask.Answer(question, cancellation));

await app.Services.GetRequiredService<ChunkStore>().EnsureExists();

await app.RunAsync();
