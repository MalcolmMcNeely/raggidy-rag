namespace RaggidyRag.Api.Asking;

public sealed class ClaudeOptions
{
    public string ApiKey { get; set; } = "";

    public string Model { get; set; } = "claude-sonnet-5-5";

    public int MaxTokens { get; set; } = 1024;

    public TimeSpan Patience { get; set; } = TimeSpan.FromMinutes(2);
}
