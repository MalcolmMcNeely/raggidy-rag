namespace RaggidyRag.Api.Asking;

public sealed record RetrievedChunk(int Number, string DocumentPath, string HeadingTrail, double Score, string Text);
