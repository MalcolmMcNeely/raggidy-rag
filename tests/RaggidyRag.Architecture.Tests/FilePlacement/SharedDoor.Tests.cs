namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class SharedDoorTests
{
    [Fact]
    public void EveryFolderInSharedNamesAWordOfTheGlossary()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);

        var found = new List<string>();
        foreach (var (root, context) in slices.Roots)
        {
            foreach (var folder in slices.FoldersWithSource(repo, root))
            {
                var parts = folder.Split('/');
                if (parts.Length == 2 && slices.IsShared(root, parts[0]) && !context.Glossary.Names(parts[1]))
                {
                    found.Add($"{root.Folder}/{folder}/ names no word in the glossary of {context.Name}");
                }
            }
        }

        Breaches.None(found);
    }
}
