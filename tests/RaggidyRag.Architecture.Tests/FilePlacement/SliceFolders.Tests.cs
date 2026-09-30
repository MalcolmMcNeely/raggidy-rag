namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class SliceFoldersTests
{
    [Fact]
    public void EveryFirstLevelFolderUnderACodeRootIsASliceOrShared()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);

        var found = new List<string>();
        foreach (var (root, _) in slices.Roots)
        {
            foreach (var folder in slices.FoldersWithSource(repo, root))
            {
                var name = Repo.NameOf(folder);
                if (Slices.DepthOf(folder) == 1 && !slices.IsSlice(root, name) && !slices.IsShared(root, name))
                {
                    found.Add($"{root.Folder}/{folder}/ is neither a Slice in slices nor {slices.SharedName(root)}");
                }

                if (Slices.DepthOf(folder) > 1 && name.Equals("shared", StringComparison.OrdinalIgnoreCase))
                {
                    found.Add($"{root.Folder}/{folder}/ is Shared below the first level");
                }
            }
        }

        Breaches.None(found);
    }

    [Fact]
    public void EveryFolderInsideAFrontEndSliceIsAConcern()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        var fronts = slices.Roots.Where(pair => pair.Root.IsFrontEnd).ToList();
        Assert.SkipWhen(fronts.Count == 0, "no front-end code root is in a context that declares Slices");

        var found = new List<string>();
        foreach (var (root, _) in fronts)
        {
            foreach (var folder in slices.FoldersWithSource(repo, root))
            {
                var parts = folder.Split('/');
                if (parts.Length == 2 && slices.IsSlice(root, parts[0]) && !slices.Concerns.Contains(parts[1], StringComparer.Ordinal))
                {
                    found.Add($"{root.Folder}/{folder}/ is not a Concern in concerns");
                }
            }
        }

        Breaches.None(found);
    }

    [Fact]
    public void ASliceFolderSitsAtTheFirstLevelUnderItsOwnNameInEveryCodeRoot()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);

        var found = new List<string>();
        foreach (var (root, _) in slices.Roots)
        {
            foreach (var folder in slices.FoldersWithSource(repo, root))
            {
                var name = Repo.NameOf(folder);
                var slice = slices.Names.FirstOrDefault(candidate => candidate.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (slice is null)
                {
                    continue;
                }

                if (Slices.DepthOf(folder) != 1)
                {
                    found.Add($"{root.Folder}/{folder}/ carries the Slice name {slice} below the first level");
                }
                else if (name != slices.Expected(root, slice))
                {
                    found.Add($"{root.Folder}/{folder}/ is the Slice {slice} in the wrong case, expected {slices.Expected(root, slice)}");
                }
            }
        }

        Breaches.None(found);
    }

    [Fact]
    public void EverySliceIsAHeadwordOfTheGlossary()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);
        var glossaries = slices.Roots.Select(pair => pair.Context).Distinct().Select(context => context.Glossary).ToList();

        var found = slices.Names
            .Where(slice => !glossaries.Any(glossary => glossary.Headwords.Any(headword => headword.Replace(" ", "", StringComparison.Ordinal) == slice)))
            .Select(slice => $"{slice} is in slices, and no glossary has it as a headword")
            .ToList();

        Breaches.None(found);
    }
}
