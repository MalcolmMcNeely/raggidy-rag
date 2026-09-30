using System.Linq.Expressions;
using Microsoft.Extensions.VectorData;
using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Tests.Hosting;

// A running Postgres container cannot be made to fail on demand, so the store fails here instead.
public sealed class FailingChunks : VectorStoreCollection<string, Chunk>
{
    public override string Name => ChunkStore.CollectionName;

    // The API makes sure the collection exists as it starts, and a host that never starts proves nothing.
    public override Task EnsureCollectionExistsAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public override Task<bool> CollectionExistsAsync(CancellationToken cancellationToken = default) => throw Refused();

    public override Task EnsureCollectionDeletedAsync(CancellationToken cancellationToken = default) => throw Refused();

    public override Task<Chunk?> GetAsync(string key, RecordRetrievalOptions? options = null, CancellationToken cancellationToken = default) =>
        throw Refused();

    public override IAsyncEnumerable<Chunk> GetAsync(
        Expression<Func<Chunk, bool>> filter, int top, FilteredRecordRetrievalOptions<Chunk>? options = null, CancellationToken cancellationToken = default) =>
        throw Refused();

    public override Task DeleteAsync(string key, CancellationToken cancellationToken = default) => throw Refused();

    public override Task UpsertAsync(Chunk record, CancellationToken cancellationToken = default) => throw Refused();

    public override Task UpsertAsync(IEnumerable<Chunk> records, CancellationToken cancellationToken = default) => throw Refused();

    public override IAsyncEnumerable<VectorSearchResult<Chunk>> SearchAsync<TInput>(
        TInput searchValue, int top, VectorSearchOptions<Chunk>? options = null, CancellationToken cancellationToken = default) =>
        throw Refused();

    public override object? GetService(Type serviceType, object? serviceKey = null) => null;

    static VectorStoreException Refused() => new("Postgres refused the connection.");
}
