namespace RaggidyRag.Architecture.Tests.Words;

public sealed class BannedWordsTests
{
    [Fact]
    public void NoSourceFileUsesAWordThatLost()
    {
        var repo = Repo.Find();
        var rule = repo.Rule("words");
        var lists = rule.Lists("banned-words");
        var skipped = rule.Strings("skip-folders");
        var map = ContextMap.Read(repo);

        var found = lists.Keys
            .Where(name => map.Contexts.All(context => context.Name != name))
            .Select(name => $"banned-words names {name}, and no context carries that name")
            .ToList();
        var patterns = lists.ToDictionary(list => list.Key, list => list.Value.Select(entry => (Word: entry, Pattern: PatternOf(entry))).ToList());
        foreach (var file in repo.SourceFiles.Where(file => !IsHistory(file, skipped)))
        {
            var context = map.Claiming(file.Relative);
            if (context is null || !patterns.TryGetValue(context.Name, out var banned))
            {
                continue;
            }

            foreach (var (word, pattern) in banned)
            {
                var match = pattern.Match(file.Text);
                if (match.Success)
                {
                    found.Add($"{file}:{LineOf(file.Text, match.Index)} uses \"{match.Value}\", and {word} lost in {context.Name}");
                }
            }
        }

        Assert.SkipWhen(found.Count == 0 && lists.Values.All(list => list.Count == 0), "banned-words holds no word, so the rule judges nothing");
        Breaches.None(found);
    }

    // The words of a name run together by nothing, an underscore, a hyphen or one space, and a
    // change of case is a word boundary too, so OrderLine, order_line and MyOrderLine all match.
    static Regex PatternOf(string entry)
    {
        var words = entry.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(AnyCase);
        var opens = @"(?:(?<![\p{L}\p{N}])|(?<=[\p{Ll}\p{N}])(?=\p{Lu}))";
        var closes = @"(?:(?![\p{L}\p{N}])|(?<=\p{Ll})(?=\p{Lu}))";
        return new Regex(opens + string.Join("[ _-]?", words) + closes);
    }

    static string AnyCase(string word) => string.Concat(word.Select(letter => char.IsLetter(letter)
        ? $"[{char.ToLowerInvariant(letter)}{char.ToUpperInvariant(letter)}]"
        : Regex.Escape(letter.ToString())));

    static bool IsHistory(SourceFile file, IReadOnlyList<string> skipped) =>
        Repo.Ancestors(file.Folder).Any(folder => skipped.Any(skip => skip.Trim('/') == folder || skip == Repo.NameOf(folder)));

    static int LineOf(string text, int index) => text.AsSpan(0, index).Count('\n') + 1;
}
