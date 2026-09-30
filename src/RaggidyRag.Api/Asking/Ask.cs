using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Asking;

public sealed class Ask(ChunkStore chunks, Retrieve retrieve)
{
    public const string NothingIngestedYet = "Nothing has been ingested yet. Press Ingest first.";

    public async Task<AskResult> Answer(Question question, CancellationToken cancellation)
    {
        if (!await chunks.HoldsAnyChunk(cancellation))
        {
            return new AskResult(NothingIngestedYet, [], []);
        }

        var retrieved = await retrieve.Run(question, cancellation);
        return new AskResult("", [], retrieved);
    }
}
