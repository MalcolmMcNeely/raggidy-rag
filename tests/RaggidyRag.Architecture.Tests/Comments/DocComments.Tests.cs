namespace RaggidyRag.Architecture.Tests.Comments;

public sealed class DocCommentsTests
{
    [Fact]
    public void DocCommentsSitWhereTheSettingSays()
    {
        var repo = Repo.Find();
        var allowedInCode = repo.Rule("comments").Bool("doc-comments");

        var found = repo.SourceFiles
            .Where(file => !allowedInCode || file.IsTest)
            .SelectMany(file => DocCommentLines(file).Select(line => $"{file}:{line} holds a doc comment"))
            .ToList();

        Breaches.None(found);
    }

    static IEnumerable<int> DocCommentLines(SourceFile file) => file.Extension switch
    {
        ".cs" => CSharp(file),
        ".ts" or ".tsx" or ".mjs" => TypeScript(file),
        ".py" => Python(file),
        _ => [],
    };

    // Roslyn reads //// and /**/ as ordinary comments.
    static IEnumerable<int> CSharp(SourceFile file)
    {
        var at = 0;
        foreach (var line in file.Lines)
        {
            at++;
            var start = line.TrimStart();
            var single = start.StartsWith("///", StringComparison.Ordinal) && !start.StartsWith("////", StringComparison.Ordinal);
            var block = start.StartsWith("/**", StringComparison.Ordinal) && !start.StartsWith("/**/", StringComparison.Ordinal);
            if (single || block)
            {
                yield return at;
            }
        }
    }

    static IEnumerable<int> TypeScript(SourceFile file)
    {
        var lines = file.Lines.ToList();
        for (var at = 0; at < lines.Count; at++)
        {
            var start = lines[at].TrimStart();
            if (!start.StartsWith("/**", StringComparison.Ordinal) || start.StartsWith("/**/", StringComparison.Ordinal))
            {
                continue;
            }

            var opened = at;
            var inner = new List<string>();
            var piece = start[3..];
            while (true)
            {
                var close = piece.IndexOf("*/", StringComparison.Ordinal);
                inner.Add(close < 0 ? piece : piece[..close]);
                if (close >= 0 || ++at >= lines.Count)
                {
                    break;
                }

                piece = lines[at];
            }

            var text = inner.Select(line => line.Trim().TrimStart('*').Trim()).Where(line => line.Length > 0);
            if (text.Any(line => !line.StartsWith('@')))
            {
                yield return opened + 1;
            }
        }
    }

    static IEnumerable<int> Python(SourceFile file)
    {
        var lines = file.Lines.ToList();
        string? open = null;
        for (var at = 0; at < lines.Count; at++)
        {
            var line = lines[at];
            var from = 0;
            while (true)
            {
                if (open is not null)
                {
                    var close = line.IndexOf(open, from, StringComparison.Ordinal);
                    if (close < 0)
                    {
                        break;
                    }

                    open = null;
                    from = close + 3;
                    continue;
                }

                var next = NextTripleQuote(line, from, out var mark);
                if (next < 0)
                {
                    break;
                }

                open = mark;
                if (line[..next].Trim().Length == 0 && OpensADefinition(lines, at))
                {
                    yield return at + 1;
                }

                from = next + 3;
            }
        }
    }

    static bool OpensADefinition(List<string> lines, int at)
    {
        for (var above = at - 1; above >= 0; above--)
        {
            var text = lines[above].Trim();
            if (text.Length == 0 || text.StartsWith('#'))
            {
                continue;
            }

            return text.EndsWith(':');
        }

        return true;
    }

    static int NextTripleQuote(string line, int from, out string mark)
    {
        var doubled = line.IndexOf("\"\"\"", from, StringComparison.Ordinal);
        var single = line.IndexOf("'''", from, StringComparison.Ordinal);
        if (doubled < 0 || (single >= 0 && single < doubled))
        {
            mark = "'''";
            return single;
        }

        mark = "\"\"\"";
        return doubled;
    }
}
