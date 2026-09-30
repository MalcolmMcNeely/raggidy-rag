using RaggidyRag.Api.Chunking;

namespace RaggidyRag.Api.Tests.Chunking;

public sealed class ChunkerTests
{
    [Fact]
    public void ADocumentBecomesOneChunkWithItsTitleAsTheHeadingTrail()
    {
        // Arrange
        var text = "# Use sagas\n\nA saga holds a long-running process.\n";
        var document = new Document("adr/0001-use-sagas.md", text);

        // Act
        var chunks = new Chunker().Cut(document);

        // Assert
        Assert.Equal(
            [("adr/0001-use-sagas.md#0", "adr/0001-use-sagas.md", "Use sagas", 0, text)],
            chunks.Select(chunk => (chunk.Id, chunk.DocumentPath, chunk.HeadingTrail, chunk.Position, chunk.Text)));
    }

    [Fact]
    public void ADocumentWithNoTitleTakesItsFileNameAsTheHeadingTrail()
    {
        // Arrange
        var document = new Document("notes/untitled.md", "Some text under no heading.\n");

        // Act
        var chunks = new Chunker().Cut(document);

        // Assert
        Assert.Equal(["untitled.md"], chunks.Select(chunk => chunk.HeadingTrail));
    }
}
