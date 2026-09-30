using System.Net.Http.Json;

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
        Assert.Equal("""{"documentsRead":2,"chunksStored":2}""", await response.Content.ReadAsStringAsync(cancellation));
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
}
