using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using RaggidyRag.Api.Asking;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class CitationTests(ApiHost api) : IClassFixture<ApiHost>
{
    const string SetAnswer = "An inbox drops a message it has handled before [2]. It does so on every delivery [2].";

    [Fact]
    public async Task EachNumberTheAnswerUsesGetsOneCitationWithItsDocumentPathAndHeadingTrail()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;

        // Act
        var result = await Ask("What does an outbox hold?", cancellation);

        // Assert
        Assert.Equal([new Citation(2, "inbox.md", "Inbox")], result.Citations);
    }

    [Fact]
    public async Task TheAnswerKeepsItsCitationMarks()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;

        // Act
        var result = await Ask("What does an outbox hold?", cancellation);

        // Assert
        Assert.Equal(SetAnswer, result.Answer);
    }

    async Task<AskResult> Ask(string question, CancellationToken cancellation)
    {
        using var client = api.WithDocuments("TwoTopics")
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                services.AddSingleton<IChatClient>(new AnsweringClaude(SetAnswer))))
            .CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);
        using var response = await client.PostAsJsonAsync("/ask", new { text = question }, cancellation);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AskResult>(cancellation))!;
    }
}
