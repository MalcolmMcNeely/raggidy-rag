using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunks;
using RaggidyRag.Api.Embedding;
using RaggidyRag.Api.Failures;
using RaggidyRag.Api.Tracing;

namespace RaggidyRag.Api.Asking;

public sealed class Retrieve(
    IOptions<RetrieveOptions> options,
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    ChunkStore store)
{
    public async Task<IReadOnlyList<RetrievedChunk>> Run(Question question, CancellationToken cancellation)
    {
        ReadOnlyMemory<float> vector;
        using (Steps.Source.StartActivity(Steps.Embed))
        {
            vector = await ServiceFailed.Blame(
                ServiceFailed.Voyage, () => embeddings.GenerateVectorAsync(question.Text, InputType.Query, cancellation), cancellation);
        }

        using var retrieving = Steps.Source.StartActivity(Steps.Retrieve);
        var nearest = await store.Nearest(vector, options.Value.Count, cancellation);
        return nearest
            .Select((found, index) => new RetrievedChunk(
                index + 1, found.Record.DocumentPath, found.Record.HeadingTrail, found.Score ?? throw new InvalidOperationException("The store returned a Chunk with no score."),
                found.Record.Text))
            .ToList();
    }
}
