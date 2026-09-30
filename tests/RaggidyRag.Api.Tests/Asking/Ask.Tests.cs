using System.Net.Http.Json;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class AskTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task AskBeforeAnyIngestSaysNothingHasBeenIngestedYet()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/ask", new { text = "What is a saga?" }, cancellation);

        // Assert
        Assert.Equal(
            """{"answer":"Nothing has been ingested yet. Press Ingest first.","citations":[],"retrievedChunks":[]}""",
            await response.Content.ReadAsStringAsync(cancellation));
    }
}
