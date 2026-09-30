namespace RaggidyRag.Api.Ingesting;

public sealed record IngestResult(int DocumentsRead, int ChunksStored);
