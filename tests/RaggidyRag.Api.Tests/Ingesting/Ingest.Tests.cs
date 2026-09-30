using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using RaggidyRag.Api.Asking;

namespace RaggidyRag.Api.Tests.Ingesting;

public sealed class IngestTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task IngestReportsTheDocumentsReadAndTheChunksStoredFromEveryDepthOfTheFolder()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Nested").CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Assert
        Assert.Equal("""{"documentsRead":2,"chunksStored":3}""", await response.Content.ReadAsStringAsync(cancellation));
    }

    [Fact]
    public async Task IngestStoresOneChunkForEachSectionOfADocument()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Sectioned").CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Assert
        Assert.Equal("""{"documentsRead":1,"chunksStored":4}""", await response.Content.ReadAsStringAsync(cancellation));
    }

    [Fact]
    public async Task IngestSkipsAFileThatIsNotMarkdown()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("OtherKinds").CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Assert
        Assert.Equal("""{"documentsRead":1,"chunksStored":1}""", await response.Content.ReadAsStringAsync(cancellation));
    }

    [Fact]
    public async Task IngestRunTwiceOnTheSameFolderStoresEachChunkOnce()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics")
            .WithWebHostBuilder(builder => builder.UseSetting("Retrieve:Count", "100"))
            .CreateClient();
        using var first = await client.PostAsJsonAsync("/ingest", new { }, cancellation);
        using var second = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var result = await Ask(client, "What does an outbox hold?", cancellation);

        // Assert
        string[] topics = ["inbox.md", "outbox.md", "retry.md"];
        var retrieved = result.RetrievedChunks.Select(chunk => chunk.DocumentPath).Where(topics.Contains).Order(StringComparer.Ordinal);
        Assert.Equal(topics, retrieved);
    }

    [Fact]
    public async Task IngestRunAgainAfterADocumentGotShorterLeavesNoChunkOfTheRemovedSection()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var longer = api.WithDocuments("Longer").CreateClient();
        using var shorter = api.WithDocuments("Shorter")
            .WithWebHostBuilder(builder => builder.UseSetting("Retrieve:Count", "100"))
            .CreateClient();
        using var first = await longer.PostAsJsonAsync("/ingest", new { }, cancellation);
        using var second = await shorter.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var result = await Ask(shorter, "When does a saga wake?", cancellation);

        // Assert
        var trails = result.RetrievedChunks.Where(chunk => chunk.DocumentPath == "saga-timeouts.md").Select(chunk => chunk.HeadingTrail).Order(StringComparer.Ordinal);
        Assert.Equal(["Saga timeouts", "Saga timeouts > Request"], trails);
    }

    static async Task<AskResult> Ask(HttpClient client, string question, CancellationToken cancellation)
    {
        using var response = await client.PostAsJsonAsync("/ask", new { text = question }, cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AskResult>(cancellation))!;
    }
}
