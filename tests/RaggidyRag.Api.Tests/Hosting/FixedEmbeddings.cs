using Microsoft.Extensions.AI;

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class FixedEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
{
    public const int Dimensions = 1024;

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var embeddings = values.Select(_ => new Embedding<float>(VectorFor()));
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }

    static float[] VectorFor()
    {
        var vector = new float[Dimensions];
        vector[0] = 1f;
        return vector;
    }
}
