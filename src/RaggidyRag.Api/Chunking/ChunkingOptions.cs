namespace RaggidyRag.Api.Chunking;

public sealed class ChunkingOptions
{
    public int Cap { get; set; } = 1500;

    public int Overlap { get; set; } = 200;
}
