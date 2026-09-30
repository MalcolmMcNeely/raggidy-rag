using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Tests.Failures;

public sealed class ServiceFailedTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task AskWithVoyageFailingReturnsProblemDetailsNamingVoyage()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var ingesting = api.WithDocuments("Topics").CreateClient();
        using var ingested = await ingesting.PostAsJsonAsync("/ingest", new { }, cancellation);
        using var client = api
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services
                .AddSingleton<IEmbeddingGenerator<string, Embedding<float>>, FailingEmbeddings>()))
            .CreateClient();

        // Act
        var problem = await Problem(client, "/ask", new { text = "What does an outbox hold?" }, cancellation);

        // Assert
        Assert.Equal("Voyage failed", problem.Title);
    }

    [Fact]
    public async Task AskWithClaudeFailingReturnsProblemDetailsNamingClaude()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics")
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services
                .AddSingleton<IChatClient, FailingClaude>()))
            .CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        var problem = await Problem(client, "/ask", new { text = "What does an outbox hold?" }, cancellation);

        // Assert
        Assert.Equal("Claude failed", problem.Title);
    }

    [Fact]
    public async Task IngestWithPostgresFailingReturnsProblemDetailsNamingPostgres()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        using var client = api.WithDocuments("Topics")
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services
                .AddSingleton<VectorStoreCollection<string, Chunk>, FailingChunks>()))
            .CreateClient();

        // Act
        var problem = await Problem(client, "/ingest", new { }, cancellation);

        // Assert
        Assert.Equal("Postgres failed", problem.Title);
    }

    static async Task<ProblemDetails> Problem(HttpClient client, string path, object body, CancellationToken cancellation)
    {
        using var response = await client.PostAsJsonAsync(path, body, cancellation);
        return (await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellation))!;
    }
}
