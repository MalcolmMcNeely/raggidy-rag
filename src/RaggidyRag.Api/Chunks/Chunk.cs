using Microsoft.Extensions.VectorData;

namespace RaggidyRag.Api.Chunks;

public sealed class Chunk
{
    [VectorStoreKey(StorageName = "id")]
    public string Id { get; set; } = "";

    [VectorStoreData(StorageName = "document_path")]
    public string DocumentPath { get; set; } = "";

    [VectorStoreData(StorageName = "heading_trail")]
    public string HeadingTrail { get; set; } = "";

    [VectorStoreData(StorageName = "position")]
    public int Position { get; set; }

    [VectorStoreData(StorageName = "text")]
    public string Text { get; set; } = "";

    [VectorStoreVector(1024, DistanceFunction = DistanceFunction.CosineDistance, StorageName = "embedding")]
    public ReadOnlyMemory<float> Embedding { get; set; }
}
