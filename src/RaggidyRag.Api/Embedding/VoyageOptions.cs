namespace RaggidyRag.Api.Embedding;

public sealed class VoyageOptions
{
    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "voyage-4-lite";

    public int BatchSize { get; set; } = 128;

    public TimeSpan Patience { get; set; } = TimeSpan.FromMinutes(2);
}
