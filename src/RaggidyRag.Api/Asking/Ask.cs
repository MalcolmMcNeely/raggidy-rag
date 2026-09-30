using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Asking;

public sealed class Ask(ChunkStore chunks)
{
    public const string NothingIngestedYet = "Nothing has been ingested yet. Press Ingest first.";

    public async Task<AskResult> Answer(Question question, CancellationToken cancellation)
    {
        if (!await chunks.HoldsAnyChunk(cancellation))
        {
            return new AskResult(NothingIngestedYet, [], []);
        }

        throw new NotImplementedException("Retrieve and Answer are not built yet.");
    }
}
