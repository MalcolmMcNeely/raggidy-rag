using Microsoft.Extensions.VectorData;
using RaggidyRag.Api.Failures;

namespace RaggidyRag.Api.Chunks;

public sealed class ChunkStore(VectorStoreCollection<string, Chunk> chunks)
{
    public const string CollectionName = "chunks";

    public Task EnsureExists(CancellationToken cancellation = default) => chunks.EnsureCollectionExistsAsync(cancellation);

    public Task Replace(string documentPath, IEnumerable<Chunk> records, CancellationToken cancellation) =>
        ServiceFailed.Blame(ServiceFailed.Postgres, async () =>
        {
            var stored = await chunks
                .GetAsync(chunk => chunk.DocumentPath == documentPath, int.MaxValue, new FilteredRecordRetrievalOptions<Chunk> { IncludeVectors = false }, cancellation)
                .Select(chunk => chunk.Id)
                .ToListAsync(cancellation);
            await chunks.DeleteAsync(stored, cancellation);
            await chunks.UpsertAsync(records, cancellation);
        }, cancellation);

    public Task<bool> HoldsAnyChunk(CancellationToken cancellation) =>
        ServiceFailed.Blame(ServiceFailed.Postgres, async () =>
        {
            var any = chunks.GetAsync(_ => true, top: 1, new FilteredRecordRetrievalOptions<Chunk> { IncludeVectors = false }, cancellation);
            return await any.AnyAsync(cancellation);
        }, cancellation);

    public Task<List<VectorSearchResult<Chunk>>> Nearest(ReadOnlyMemory<float> vector, int count, CancellationToken cancellation) =>
        ServiceFailed.Blame(
            ServiceFailed.Postgres,
            () => chunks.SearchAsync(vector, count, new VectorSearchOptions<Chunk> { IncludeVectors = false }, cancellation).ToListAsync(cancellation).AsTask(),
            cancellation);
}
