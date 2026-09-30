using System.Diagnostics;

namespace RaggidyRag.Api.Tracing;

public static class Steps
{
    // The service defaults export the source named for the app, so the Aspire dashboard shows these with no wiring of their own.
    public static readonly ActivitySource Source = new(typeof(Steps).Assembly.GetName().Name!);

    public const string Chunk = "chunk";
    public const string Embed = "embed";
    public const string Store = "store";
    public const string Retrieve = "retrieve";
    public const string Answer = "answer";
}
