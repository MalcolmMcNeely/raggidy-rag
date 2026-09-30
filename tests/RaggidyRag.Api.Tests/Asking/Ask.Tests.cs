using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class AskTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task AskBeforeAnyIngestSaysNothingHasBeenIngestedYetWithoutCallingVoyageOrClaude()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services
                .AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, FailingEmbeddings>()
                .AddSingleton<IChatClient, FailingClaude>()))
            .CreateClient();

        // Act
        using var response = await client.PostAsJsonAsync("/ask", new { text = "What is a saga?" }, cancellation);

        // Assert
        Assert.Equal(
            """{"answer":"Nothing has been ingested yet. Press Ingest first.","citations":[],"retrievedChunks":[]}""",
            await response.Content.ReadAsStringAsync(cancellation));
    }
}
