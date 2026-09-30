using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace RaggidyRag.Api.Embedding;

public sealed class VoyageEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    readonly HttpClient http;
    readonly VoyageOptions settings;
    readonly TimeProvider clock;

    public VoyageEmbeddingGenerator(HttpClient http, IOptions<VoyageOptions> options, TimeProvider clock)
    {
        this.http = http;
        settings = options.Value;
        this.clock = clock;
        http.BaseAddress = new Uri("https://api.voyageai.com/");
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        // HttpClient's own 100 second limit runs on the machine's clock, and the wait on Voyage has to be the Clock's alone.
#pragma warning disable RS0030
        http.Timeout = Timeout.InfiniteTimeSpan;
#pragma warning restore RS0030
    }

    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var inputType = options?.AdditionalProperties?.TryGetValue(InputType.Key, out string? found) == true ? found : null;
        var embeddings = new GeneratedEmbeddings<Embedding<float>>();
        foreach (var batch in values.Chunk(settings.BatchSize))
        {
            var response = await Post(new Request(batch, settings.Model, inputType), cancellationToken);
            embeddings.AddRange(response.Data.Select(embedded => new Embedding<float>(embedded.Embedding)));
        }

        return embeddings;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    // The HttpClient is the factory's to dispose.
    public void Dispose()
    {
    }

    async Task<Response> Post(Request request, CancellationToken cancellation)
    {
        using var patience = new CancellationTokenSource(settings.Patience, clock);
        using var either = CancellationTokenSource.CreateLinkedTokenSource(cancellation, patience.Token);
        using var response = await http.PostAsJsonAsync("v1/embeddings", request, Json, either.Token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Response>(Json, either.Token)
            ?? throw new InvalidOperationException("Voyage answered with an empty body.");
    }

    sealed record Request(IReadOnlyList<string> Input, string Model, string? InputType);

    sealed record Response(IReadOnlyList<Embedded> Data);

    sealed record Embedded(float[] Embedding);
}
