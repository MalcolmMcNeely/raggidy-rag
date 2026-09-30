using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using RaggidyRag.Api.Asking;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class RetrieveTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task AQuestionAboutOneDocumentRetrievesThatDocumentsChunkFirst()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics").CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var result = await Ask(client, "What does an outbox hold?", cancellation);

        // Assert
        Assert.Equal(
            new RetrievedChunk(1, "outbox.md", "Outbox", 0, "An outbox holds each message until the transaction that made it commits."),
            result.RetrievedChunks[0]);
    }

    [Fact]
    public async Task TheRetrievedChunksAreNumberedFromOneUpwardNearestFirst()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics").CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var result = await Ask(client, "What does an outbox hold?", cancellation);

        // Assert
        Assert.Equal([(1, 0.0), (2, 1.0), (3, 1.0)], result.RetrievedChunks.Select(chunk => (chunk.Number, chunk.Score)));
    }

    [Fact]
    public async Task RetrieveGetsAsManyChunksAsTheSettingSays()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics")
            .WithWebHostBuilder(builder => builder.UseSetting("Retrieve:Count", "1"))
            .CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var result = await Ask(client, "What does an outbox hold?", cancellation);

        // Assert
        Assert.Single(result.RetrievedChunks);
    }

    [Fact]
    public async Task TheQuestionGoesToVoyageAsAQuery()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics").CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        await Ask(client, "How often does a retry run?", cancellation);

        // Assert
        Assert.Contains(("How often does a retry run?", "query"), api.Embeddings.Embedded);
    }

    static async Task<AskResult> Ask(HttpClient client, string question, CancellationToken cancellation)
    {
        using var response = await client.PostAsJsonAsync("/ask", new { text = question }, cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AskResult>(cancellation))!;
    }
}
