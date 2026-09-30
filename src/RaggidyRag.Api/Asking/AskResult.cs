namespace RaggidyRag.Api.Asking;

public sealed record AskResult(string Answer, IReadOnlyList<Citation> Citations, IReadOnlyList<RetrievedChunk> RetrievedChunks);
