using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Chunking;

public sealed class Chunker
{
    public IReadOnlyList<Chunk> Cut(Document document)
    {
        var chunk = new Chunk
        {
            Id = $"{document.Path}#0",
            DocumentPath = document.Path,
            HeadingTrail = TitleOf(document),
            Position = 0,
            Text = document.Text,
        };
        return [chunk];
    }

    static string TitleOf(Document document)
    {
        var title = document.Text.Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .FirstOrDefault(line => line.StartsWith("# ", StringComparison.Ordinal));
        return title?["# ".Length..].Trim() ?? Path.GetFileName(document.Path);
    }
}
