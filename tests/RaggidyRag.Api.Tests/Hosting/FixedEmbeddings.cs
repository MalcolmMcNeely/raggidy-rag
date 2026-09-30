using System.Collections.Concurrent;
using Microsoft.Extensions.AI;

namespace RaggidyRag.Api.Tests.Hosting;

// A text that names a topic gets that topic's vector, so a Question lands at distance 0 from its topic's Chunk and 1 from the rest.
public sealed class FixedEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
{
    public const int Dimensions = 1024;

    static readonly string[] Topics = ["inbox", "outbox", "retry"];

    readonly ConcurrentQueue<(string Text, string? InputType)> embedded = new();

    public IReadOnlyCollection<(string Text, string? InputType)> Embedded => embedded;

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var inputType = options?.AdditionalProperties?.TryGetValue("input_type", out string? found) == true ? found : null;
        var embeddings = values.Select(text =>
        {
            embedded.Enqueue((text, inputType));
            return new Embedding<float>(VectorFor(text));
        });
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings.ToList()));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    static float[] VectorFor(string text)
    {
        var vector = new float[Dimensions];
        var topic = Array.FindIndex(Topics, topic => text.Contains(topic, StringComparison.OrdinalIgnoreCase));
        vector[topic + 1] = 1f;
        return vector;
    }
}
