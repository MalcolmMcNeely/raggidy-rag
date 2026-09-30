using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace RaggidyRag.Api.Tests.Asking;

public sealed class ClaudeOptionsTests(ApiHost api) : IClassFixture<ApiHost>
{
    [Fact]
    public async Task TheWaitOnClaudeEndsWhenTheClockPassesThePatience()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        var claude = new HoldingClaude();
        using var client = api.WithDocuments("TwoTopics")
            .WithWebHostBuilder(builder => builder
                .UseSetting("Claude:Patience", "00:00:30")
                .ConfigureTestServices(services => services.AddSingleton<IChatClient>(claude)))
            .CreateClient();
        using var ingested = await client.PostAsJsonAsync("/ingest", new { }, cancellation);
        var asking = client.PostAsJsonAsync("/ask", new { text = "What does an outbox hold?" }, cancellation);
        await claude.Holding.WaitAsync(cancellation);

        // Act
        api.Clock.Advance(TimeSpan.FromSeconds(31));

        // Assert
        using var response = await asking;
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }
}
