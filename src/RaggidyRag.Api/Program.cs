using Anthropic;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using RaggidyRag.Api.Asking;
using RaggidyRag.Api.Chunking;
using RaggidyRag.Api.Chunks;
using RaggidyRag.Api.Embedding;
using RaggidyRag.Api.Failures;
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
builder.Services.TryAddSingleton(services => services.GetRequiredService<VectorStore>().GetCollection<string, Chunk>(ChunkStore.CollectionName));
builder.Services.AddSingleton<ChunkStore>();
builder.Services.AddSingleton<Chunker>();

builder.Services.AddOptions<VoyageOptions>().BindConfiguration("Voyage");
builder.Services.AddOptions<IngestOptions>().BindConfiguration("Ingest");
builder.Services.AddOptions<RetrieveOptions>().BindConfiguration("Retrieve");
// A Chunk after the first must hold more than the overlap it repeats, or the cut never moves on.
builder.Services.AddOptions<ChunkingOptions>().BindConfiguration("Chunking")
    .Validate(chunking => chunking.Overlap >= 0 && chunking.Overlap < chunking.Cap, "Chunking:Overlap must be at least 0 and less than Chunking:Cap.")
    .ValidateOnStart();

// The default resilience handler has timeouts of its own, and the Clock's wait on Voyage must be the only one.
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>, VoyageEmbeddingGenerator>()
    .RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001

builder.Services.AddOptions<ClaudeOptions>().BindConfiguration("Claude");
#pragma warning disable EXTEXP0001
builder.Services.AddHttpClient("Claude").RemoveAllResilienceHandlers();
#pragma warning restore EXTEXP0001
builder.Services.AddSingleton<IChatClient>(services =>
{
    var claude = services.GetRequiredService<IOptions<ClaudeOptions>>().Value;
    var http = services.GetRequiredService<IHttpClientFactory>().CreateClient("Claude");

    // HttpClient's and the Anthropic client's own limits run on the machine's clock, and the wait on Claude has to be the Clock's alone.
#pragma warning disable RS0030
    http.Timeout = Timeout.InfiniteTimeSpan;
#pragma warning restore RS0030
    return new AnthropicClient { ApiKey = claude.ApiKey, HttpClient = http, Timeout = Timeout.InfiniteTimeSpan }
        .AsIChatClient(claude.Model);
});

builder.Services.AddScoped<Ingest>();
builder.Services.AddScoped<Retrieve>();
builder.Services.AddSingleton<PromptBuilder>();
builder.Services.AddScoped<Ask>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ServiceFailedHandler>();

var app = builder.Build();

app.UseExceptionHandler();
app.MapDefaultEndpoints();

app.MapPost("/ingest", (Ingest ingest, CancellationToken cancellation) => ingest.Run(cancellation));
app.MapPost("/ask", (Question question, Ask ask, CancellationToken cancellation) => ask.Answer(question, cancellation));

await app.Services.GetRequiredService<ChunkStore>().EnsureExists();

await app.RunAsync();
