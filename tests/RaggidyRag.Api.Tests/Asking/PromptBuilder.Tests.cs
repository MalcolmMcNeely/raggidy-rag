using System.Net.Http.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class PromptBuilderTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task ThePromptHoldsTheInstructionsThenEachRetrievedChunkThenTheQuestion()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        var claude = new AnsweringClaude("An outbox holds each message [1].");
        using var client = api.WithDocuments("TwoTopics")
            .WithWebHostBuilder(builder => builder.ConfigureTestServices(services => services.AddSingleton<IChatClient>(claude)))
            .CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);

        // Act
        using var asked = await client.PostAsJsonAsync("/ask", new { text = "What does an outbox hold?" }, cancellation);

        // Assert
        // A raw string takes the line endings of the checkout, and the Prompt must read the same on every OS.
        Assert.Equal(
            """
            Answer the Question from the Chunks below, and from nothing else.
            After each fact, put the number of the Chunk that supports it in square brackets, such as [2].
            When the Chunks do not answer the Question, say that the documents do not say.

            Chunk [1]
            Document: outbox.md
            Heading trail: Outbox
            An outbox holds each message until the transaction that made it commits.

            Chunk [2]
            Document: inbox.md
            Heading trail: Inbox
            An inbox drops each message it has handled before.

            Question: What does an outbox hold?
            """.ReplaceLineEndings("\n"),
            Assert.Single(claude.Prompts));
    }
}
