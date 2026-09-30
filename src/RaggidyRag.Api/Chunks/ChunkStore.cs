using Microsoft.Extensions.VectorData;

namespace RaggidyRag.Api.Chunks;

public sealed class ChunkStore(VectorStore store)
{
    readonly VectorStoreCollection<string, Chunk> chunks = store.GetCollection<string, Chunk>("chunks");

    public Task EnsureExists(CancellationToken cancellation = default) => chunks.EnsureCollectionExistsAsync(cancellation);

    public Task Store(IEnumerable<Chunk> records, CancellationToken cancellation) => chunks.UpsertAsync(records, cancellation);

    public async Task<bool> HoldsAnyChunk(CancellationToken cancellation)
    {
        var any = chunks.GetAsync(_ => true, top: 1, new FilteredRecordRetrievalOptions<Chunk> { IncludeVectors = false }, cancellation);
        return await any.AnyAsync(cancellation);
    }

    public async Task<IReadOnlyList<VectorSearchResult<Chunk>>> Nearest(ReadOnlyMemory<float> vector, int count, CancellationToken cancellation) =>
        await chunks.SearchAsync(vector, count, new VectorSearchOptions<Chunk> { IncludeVectors = false }, cancellation).ToListAsync(cancellation);
}
