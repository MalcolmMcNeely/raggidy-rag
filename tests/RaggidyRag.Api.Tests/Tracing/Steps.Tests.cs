using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using RaggidyRag.Api.Tracing;

namespace RaggidyRag.Api.Tests.Tracing;

public sealed class StepsTests(ApiHost api) : IClassFixture<ApiHost>
{
    const string RequestIn = "Microsoft.AspNetCore.Hosting.HttpRequestIn";

    [Fact]
    public async Task AnIngestAndAnAskTraceTheChunkEmbedStoreRetrieveAndAnswerSteps()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var recorder = new Recorder();
        using var client = api.WithDocuments("Topics").CreateClient();
        var ingestTrace = ActivityTraceId.CreateFromString("1a000000000000000000000000000001");
        var askTrace = ActivityTraceId.CreateFromString("1a000000000000000000000000000002");

        // Act
        using var ingest = await client.SendAsync(Traced(HttpMethod.Post, "/ingest", new { }, ingestTrace), cancellation);
        using var ask = await client.SendAsync(Traced(HttpMethod.Post, "/ask", new { text = "How does the outbox work?" }, askTrace), cancellation);

        // Assert
        Assert.Equal(
            ["answer", "chunk", "embed", "retrieve", "store"],
            recorder.StepsIn(ingestTrace).Concat(recorder.StepsIn(askTrace)).Select(step => step.OperationName).Distinct().Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task EachStepNestsUnderTheRequestThatRanIt()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var recorder = new Recorder();
        using var client = api.WithDocuments("Topics").CreateClient();
        var ingestTrace = ActivityTraceId.CreateFromString("1b000000000000000000000000000001");
        var askTrace = ActivityTraceId.CreateFromString("1b000000000000000000000000000002");

        // Act
        using var ingest = await client.SendAsync(Traced(HttpMethod.Post, "/ingest", new { }, ingestTrace), cancellation);
        using var ask = await client.SendAsync(Traced(HttpMethod.Post, "/ask", new { text = "How does the outbox work?" }, askTrace), cancellation);

        // Assert
        Assert.Equal(
            ["ask answer", "ask embed", "ask retrieve", "ingest chunk", "ingest embed", "ingest store"],
            recorder.StepsIn(ingestTrace).Select(step => (request: "ingest", step)).Concat(recorder.StepsIn(askTrace).Select(step => (request: "ask", step)))
                .Where(pair => pair.step.Parent?.OperationName == RequestIn)
                .Select(pair => $"{pair.request} {pair.step.OperationName}")
                .Order(StringComparer.Ordinal));
    }

    static HttpRequestMessage Traced(HttpMethod method, string path, object body, ActivityTraceId trace) =>
        new(method, path)
        {
            Content = JsonContent.Create(body),
            Headers = { { "traceparent", $"00-{trace.ToHexString()}-00f067aa0ba902b7-01" } },
        };

    sealed class Recorder : IDisposable
    {
        // Read before the listener exists: a listener that touches Steps while Steps builds its source fails its static setup.
        readonly string stepsSource = Steps.Source.Name;
        readonly ConcurrentQueue<Activity> stopped = new();
        readonly ActivityListener listener;

        public Recorder()
        {
            listener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == stepsSource || source.Name == "Microsoft.AspNetCore",
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = stopped.Enqueue,
            };
            ActivitySource.AddActivityListener(listener);
        }

        public IEnumerable<Activity> StepsIn(ActivityTraceId trace) =>
            stopped.Where(activity => activity.Source.Name == stepsSource && activity.TraceId == trace);

        public void Dispose() => listener.Dispose();
    }
}
