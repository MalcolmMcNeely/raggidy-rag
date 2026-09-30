namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class NameMapTests
{
    [Fact]
    public void ASubjectThatMatchesAPatternSitsInThatPatternsFolder()
    {
        var repo = Repo.Find();
        var map = repo.Rule("file-placement").Map("name-map");

        var found = new List<string>();
        foreach (var file in repo.SourceFiles)
        {
            var folders = map.Where(entry => Matches(entry.Key, file.Subject)).Select(entry => entry.Value).ToList();
            if (folders.Count > 0 && !folders.Contains(Repo.NameOf(file.Folder), StringComparer.Ordinal))
            {
                found.Add($"{file} matches name-map, and sits outside {string.Join(" or ", folders)}/");
            }
        }

        Breaches.None(found);
    }

    static bool Matches(string pattern, string subject) =>
        pattern.StartsWith('*') ? subject.EndsWith(pattern[1..], StringComparison.Ordinal)
        : pattern.EndsWith('*') ? subject.StartsWith(pattern[..^1], StringComparison.Ordinal)
        : pattern == subject;
}
