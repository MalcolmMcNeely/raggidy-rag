using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunking;

namespace RaggidyRag.Api.Tests.Chunking;

public sealed class ChunkerTests
{
    [Fact]
    public void ADocumentWithHeadingsAtSeveralLevelsBecomesOneChunkPerSectionWithItsHeadingTrailFromTheTitleDown()
    {
        // Arrange
        var text = """
            # Saga model

            A saga is a long-running process.

            ## Lifetime

            A saga starts on its first event.

            ### Completion

            A saga ends when it marks itself complete.

            ## Steps

            Each step sends a command.

            """;
        var document = new Document("adr/saga-model.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal(
            [
                ("adr/saga-model.md#0", "adr/saga-model.md", "Saga model", 0, "A saga is a long-running process."),
                ("adr/saga-model.md#1", "adr/saga-model.md", "Saga model > Lifetime", 1, "A saga starts on its first event."),
                ("adr/saga-model.md#2", "adr/saga-model.md", "Saga model > Lifetime > Completion", 2, "A saga ends when it marks itself complete."),
                ("adr/saga-model.md#3", "adr/saga-model.md", "Saga model > Steps", 3, "Each step sends a command."),
            ],
            chunks.Select(chunk => (chunk.Id, chunk.DocumentPath, chunk.HeadingTrail, chunk.Position, chunk.Text)));
    }

    [Fact]
    public void AHeadingInsideAFencedCodeBlockDoesNotStartAChunk()
    {
        // Arrange
        var text = """
            # Shell

            Run it:

            ```sh
            # not a heading
            echo hi
            ```

            ## After

            Text.

            """;
        var document = new Document("shell.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal(
            [
                ("Shell", "Run it:\n\n```sh\n# not a heading\necho hi\n```"),
                ("Shell > After", "Text."),
            ],
            chunks.Select(chunk => (chunk.HeadingTrail, chunk.Text)));
    }

    [Fact]
    public void TextBeforeTheFirstHeadingGetsTheTitleAsItsHeadingTrail()
    {
        // Arrange
        var text = """
            Status: accepted.

            # Use an outbox

            An outbox holds each message until its transaction commits.

            """;
        var document = new Document("adr/0002-use-an-outbox.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal(
            [
                ("Use an outbox", "Status: accepted."),
                ("Use an outbox", "An outbox holds each message until its transaction commits."),
            ],
            chunks.Select(chunk => (chunk.HeadingTrail, chunk.Text)));
    }

    [Fact]
    public void AnEmptySectionMakesNoChunk()
    {
        // Arrange
        var text = """
            # Saga model

            ## Lifetime

            ## Steps

            Each step sends a command.

            """;
        var document = new Document("saga-model.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal(
            [("saga-model.md#0", "Saga model > Steps", 0, "Each step sends a command.")],
            chunks.Select(chunk => (chunk.Id, chunk.HeadingTrail, chunk.Position, chunk.Text)));
    }

    [Fact]
    public void ADocumentWithNoTitleTakesItsFileNameAsTheHeadingTrail()
    {
        // Arrange
        var document = new Document("notes/untitled.md", "Some text under no heading.\n");

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal(["untitled.md"], chunks.Select(chunk => chunk.HeadingTrail));
    }

    [Fact]
    public void ASectionOverTheCapIsCutAtItsParagraphBreaksAndEachChunkKeepsTheHeadingTrail()
    {
        // Arrange
        var text = """
            # Saga model

            ## Lifetime

            A saga starts on its first event.

            It lives until it completes.

            Then it is deleted.

            """;
        var document = new Document("saga-model.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions { Cap = 70, Overlap = 0 }).Cut(document);

        // Assert
        Assert.Equal(
            [
                ("Saga model > Lifetime", "A saga starts on its first event.\n\nIt lives until it completes."),
                ("Saga model > Lifetime", "Then it is deleted."),
            ],
            chunks.Select(chunk => (chunk.HeadingTrail, chunk.Text)));
    }

    [Fact]
    public void EachChunkAfterTheFirstBeginsWithTheTailOfTheChunkBeforeItOfTheOverlapsLength()
    {
        // Arrange
        var text = """
            # Saga model

            A saga starts on its first event.

            It lives until it completes.

            Then it is deleted.

            """;
        var document = new Document("saga-model.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions { Cap = 70, Overlap = 10 }).Cut(document);

        // Assert
        Assert.Equal(
            [
                "A saga starts on its first event.\n\nIt lives until it completes.",
                "completes.\n\nThen it is deleted.",
            ],
            chunks.Select(chunk => chunk.Text));
    }

    [Fact]
    public void AParagraphLongerThanTheCapByItselfIsCutAtTheCap()
    {
        // Arrange
        var document = new Document("alphabet.md", "# Alphabet\n\nabcdefghijklmnopqrstuvwxy\n");

        // Act
        var chunks = ChunkerWith(new ChunkingOptions { Cap = 10, Overlap = 0 }).Cut(document);

        // Assert
        Assert.Equal(["abcdefghij", "klmnopqrst", "uvwxy"], chunks.Select(chunk => chunk.Text));
    }

    [Fact]
    public void ASectionUnderTheCapStaysOneChunk()
    {
        // Arrange
        var text = """
            # Saga model

            A saga starts on its first event.

            It lives until it completes.

            """;
        var document = new Document("saga-model.md", text);

        // Act
        var chunks = ChunkerWith(new ChunkingOptions { Cap = 70, Overlap = 10 }).Cut(document);

        // Assert
        Assert.Equal(["A saga starts on its first event.\n\nIt lives until it completes."], chunks.Select(chunk => chunk.Text));
    }

    [Fact]
    public void UnderTheDefaultSettingsASectionOfSixteenHundredCharactersIsCutAtItsParagraphBreakWithTwoHundredCharactersOfOverlap()
    {
        // Arrange
        var first = new string('a', 800);
        var second = new string('b', 800);
        var document = new Document("letters.md", $"# Letters\n\n{first}\n\n{second}\n");

        // Act
        var chunks = ChunkerWith(new ChunkingOptions()).Cut(document);

        // Assert
        Assert.Equal([first, $"{new string('a', 200)}\n\n{second}"], chunks.Select(chunk => chunk.Text));
    }

    [Fact]
    public void RaisingTheCapMovesTheCut()
    {
        // Arrange
        var document = new Document("letters.md", $"# Letters\n\n{new string('a', 800)}\n\n{new string('b', 800)}\n");

        // Act
        var chunks = ChunkerWith(new ChunkingOptions { Cap = 2000 }).Cut(document);

        // Assert
        Assert.Single(chunks);
    }

    static Chunker ChunkerWith(ChunkingOptions settings) => new(Options.Create(settings));
}
