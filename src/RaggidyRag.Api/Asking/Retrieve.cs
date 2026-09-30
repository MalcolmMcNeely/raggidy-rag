using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunks;
using RaggidyRag.Api.Embedding;

namespace RaggidyRag.Api.Asking;

public sealed class Retrieve(
    IOptions<RetrieveOptions> options,
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    ChunkStore store)
{
    public async Task<IReadOnlyList<RetrievedChunk>> Run(Question question, CancellationToken cancellation)
    {
        var vector = await embeddings.GenerateVectorAsync(question.Text, InputType.Query, cancellation);
        var nearest = await store.Nearest(vector, options.Value.Count, cancellation);
        return nearest
            .Select((found, index) => new RetrievedChunk(
                index + 1, found.Record.DocumentPath, found.Record.HeadingTrail, found.Score ?? throw new InvalidOperationException("The store returned a Chunk with no score."),
                found.Record.Text))
            .ToList();
    }
}
