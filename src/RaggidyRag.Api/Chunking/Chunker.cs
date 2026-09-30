using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Chunking;

public sealed partial class Chunker(IOptions<ChunkingOptions> options)
{
    readonly ChunkingOptions settings = options.Value;

    public IReadOnlyList<Chunk> Cut(Document document)
    {
        var lines = LinesOf(document.Text);
        var title = lines.Select(line => line.Heading).FirstOrDefault(heading => heading?.Level == 1)?.Text
            ?? Path.GetFileName(document.Path);

        var chunks = new List<Chunk>();
        var trail = new List<Heading>();
        var body = new List<string>();
        foreach (var (line, heading) in lines)
        {
            if (heading is null)
            {
                body.Add(line);
                continue;
            }

            CloseSection();
            trail.RemoveAll(above => above.Level >= heading.Level);
            trail.Add(heading);
        }

        CloseSection();
        return chunks;

        void CloseSection()
        {
            var text = string.Join('\n', body).Trim();
            body.Clear();
            if (text.Length == 0)
            {
                return;
            }

            var headingTrail = trail.Count == 0 ? title : string.Join(" > ", trail.Select(heading => heading.Text));
            foreach (var sized in CutToCap(text))
            {
                chunks.Add(new Chunk
                {
                    Id = $"{document.Path}#{chunks.Count}",
                    DocumentPath = document.Path,
                    HeadingTrail = headingTrail,
                    Position = chunks.Count,
                    Text = sized,
                });
            }
        }
    }

    IEnumerable<string> CutToCap(string text)
    {
        var breaks = ParagraphBreak().Matches(text).Select(match => match.Index).Append(text.Length).ToList();
        var start = 0;
        var end = 0;
        while (end < text.Length)
        {
            end = breaks.LastOrDefault(at => at > end && at - start <= settings.Cap);
            if (end == 0)
            {
                end = start + settings.Cap;
            }

            yield return text[start..end];
            if (settings.Overlap > 0)
            {
                start = Math.Max(start, end - settings.Overlap);
                continue;
            }

            start = end;
            while (start < text.Length && char.IsWhiteSpace(text[start]))
            {
                start++;
            }
        }
    }

    static List<(string Line, Heading? Heading)> LinesOf(string text)
    {
        var lines = new List<(string, Heading?)>();
        string? fence = null;
        foreach (var line in text.Split('\n').Select(line => line.TrimEnd('\r')))
        {
            var marks = FenceLine().Match(line).Groups["marks"].Value;
            if (fence is null)
            {
                fence = marks.Length > 0 ? marks : null;
                lines.Add((line, fence is null ? HeadingOf(line) : null));
            }
            else
            {
                // A fence closes only on the mark that opened it, so a backtick line leaves a tilde block alone.
                fence = marks.StartsWith(fence, StringComparison.Ordinal) ? null : fence;
                lines.Add((line, null));
            }
        }

        return lines;
    }

    static Heading? HeadingOf(string line)
    {
        var match = HeadingLine().Match(line);
        return match.Success ? new Heading(match.Groups["marks"].Length, match.Groups["text"].Value) : null;
    }

    [GeneratedRegex(@"^ {0,3}(?<marks>#{1,6})(?:[ \t]+(?<text>.*?))?(?:[ \t]+#+)?[ \t]*$")]
    private static partial Regex HeadingLine();

    [GeneratedRegex(@"^ {0,3}(?<marks>`{3,}|~{3,})")]
    private static partial Regex FenceLine();

    [GeneratedRegex(@"\n[ \t]*\n")]
    private static partial Regex ParagraphBreak();

    sealed record Heading(int Level, string Text);
}
