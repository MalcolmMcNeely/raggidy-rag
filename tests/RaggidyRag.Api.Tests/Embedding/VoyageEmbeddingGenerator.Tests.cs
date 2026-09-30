using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using RaggidyRag.Api.Embedding;

namespace RaggidyRag.Api.Tests.Embedding;

public sealed class VoyageEmbeddingGeneratorTests
{
    [Fact]
    public async Task TheTextsGoToVoyageAsDocumentsInBatchesOfTheBatchSize()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        var voyage = new AnsweringVoyage();
        var settings = new VoyageOptions { ApiKey = "test-key", BatchSize = 2 };
        using var generator = new VoyageEmbeddingGenerator(new HttpClient(voyage), Options.Create(settings), new FakeTimeProvider());

        // Act
        await generator.GenerateAsync(["aa", "b", "ccc"], InputType.Document, cancellation);

        // Assert
        Assert.Equal(
            [
                """{"input":["aa","b"],"model":"voyage-4-lite","input_type":"document"}""",
                """{"input":["ccc"],"model":"voyage-4-lite","input_type":"document"}""",
            ],
            voyage.Bodies);
    }

    [Fact]
    public async Task TheVectorsComeBackOneForEachTextInOrderAcrossTheBatches()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        var settings = new VoyageOptions { ApiKey = "test-key", BatchSize = 2 };
        using var generator = new VoyageEmbeddingGenerator(new HttpClient(new AnsweringVoyage()), Options.Create(settings), new FakeTimeProvider());

        // Act
        var embeddings = await generator.GenerateAsync(["aa", "b", "ccc"], InputType.Document, cancellation);

        // Assert
        Assert.Equal([[2f], [1f], [3f]], embeddings.Select(embedding => embedding.Vector.ToArray()));
    }

    [Fact]
    public async Task TheWaitOnVoyageEndsWhenTheClockPassesThePatience()
    {
        // Arrange
        var cancellation = TestContext.Current.CancellationToken;
        var clock = new FakeTimeProvider();
        var voyage = new HoldingVoyage();
        var settings = new VoyageOptions { ApiKey = "test-key", Patience = TimeSpan.FromSeconds(30) };
        using var generator = new VoyageEmbeddingGenerator(new HttpClient(voyage), Options.Create(settings), clock);
        var generating = generator.GenerateAsync(["a"], InputType.Document, cancellation);
        await voyage.Holding.WaitAsync(cancellation);

        // Act
        clock.Advance(TimeSpan.FromSeconds(31));

        // Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => generating);
    }
}
