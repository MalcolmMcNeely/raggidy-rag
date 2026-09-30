using Microsoft.Extensions.VectorData;

namespace RaggidyRag.Api.Chunks;

public sealed class ChunkStore(VectorStore store)
{
    readonly VectorStoreCollection<string, Chunk> chunks = store.GetCollection<string, Chunk>("chunks");

    public Task EnsureExists(CancellationToken cancellation = default) => chunks.EnsureCollectionExistsAsync(cancellation);

    public async Task<bool> HoldsAnyChunk(CancellationToken cancellation)
    {
        var any = chunks.GetAsync(_ => true, top: 1, new FilteredRecordRetrievalOptions<Chunk> { IncludeVectors = false }, cancellation);
        return await any.AnyAsync(cancellation);
    }
}
