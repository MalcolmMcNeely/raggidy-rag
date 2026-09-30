namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class ContextMap
{
    static readonly Regex Entry = new(@"^\s*-\s*\[(?<name>[^\]]+)\]\((?<path>[^)]+)\)(?<rest>.*)$", RegexOptions.Multiline);
    static readonly Regex Title = new(@"^#\s+(?<name>.+?)\s*$", RegexOptions.Multiline);

    ContextMap(IReadOnlyList<Context> contexts) => Contexts = contexts;

    public IReadOnlyList<Context> Contexts { get; }

    // With no map, the one context is the whole repo, and it declares Slices when the list holds one.
    public static ContextMap Read(Repo repo)
    {
        if (repo.Exists("CONTEXT-MAP.md"))
        {
            return new ContextMap(Entry.Matches(repo.TextOf("CONTEXT-MAP.md")).Select(match => new Context(
                match.Groups["name"].Value.Trim(),
                Repo.ParentOf(Cleaned(match.Groups["path"].Value)),
                Glossary.Read(Path.Combine(repo.Root, Cleaned(match.Groups["path"].Value))),
                match.Groups["rest"].Value.Contains("slices: true", StringComparison.Ordinal))).ToList());
        }

        if (!repo.Exists("CONTEXT.md"))
        {
            return new ContextMap([]);
        }

        var name = Title.Match(repo.TextOf("CONTEXT.md")).Groups["name"].Value;
        var slicesListed = repo.Rule("file-placement").Strings("slices").Count > 0;
        return new ContextMap([new Context(name, "", Glossary.Read(Path.Combine(repo.Root, "CONTEXT.md")), slicesListed)]);
    }

    public Context? Claiming(string relativePath) =>
        Contexts.Where(context => context.Claims(relativePath)).MaxBy(context => context.Folder.Length);

    static string Cleaned(string contextPath) => contextPath.Replace('\\', '/').TrimStart('.', '/');
}
