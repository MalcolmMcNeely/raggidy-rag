namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class Glossary
{
    static readonly Regex Headword = new(@"^\*\*(.+?)\*\*:", RegexOptions.Multiline);

    Glossary(IReadOnlyList<string> headwords) => Headwords = headwords;

    public IReadOnlyList<string> Headwords { get; }

    public static Glossary Read(string path) => new(
        File.Exists(path)
            ? Headword.Matches(File.ReadAllText(path)).Select(match => match.Groups[1].Value.Trim()).ToList()
            : []);

    public bool Names(string folderName) =>
        Headwords.Any(headword => Forms(headword).Contains(folderName, StringComparer.OrdinalIgnoreCase));

    static IEnumerable<string> Forms(string headword)
    {
        var word = headword.Replace(" ", "", StringComparison.Ordinal);
        yield return word;
        yield return word + "s";
        yield return word + "es";
        if (word.EndsWith('y'))
        {
            yield return word[..^1] + "ies";
        }
    }
}
