namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class GlobalUsingsTests
{
    [Fact]
    public void NoFileHoldsOnlyUsings()
    {
        var repo = Repo.Find();

        var found = repo.SourceFiles
            .Where(file => file.Extension == ".cs")
            .Where(HoldsOnlyUsings)
            .Select(file => $"{file} holds only usings, which go in the project file as <Using> items")
            .ToList();

        Breaches.None(found);
    }

    [Fact]
    public void NoUsingItemOutsideATestProjectNamesASlice()
    {
        var repo = Repo.Find();
        var slices = repo.Rule("file-placement").Strings("slices");

        var found = new List<string>();
        foreach (var root in repo.CodeRoots.Where(root => !root.IsTestProject && !root.IsFrontEnd))
        {
            foreach (var item in root.GlobalUsings(repo.Root))
            {
                var named = slices.FirstOrDefault(slice => NamesNamespace(item, root.RootNamespace + "." + slice));
                if (named is not null)
                {
                    found.Add($"{root.Folder}/{root.ProjectName}.csproj has <Using Include=\"{item}\" />, which names the Slice {named}");
                }
            }
        }

        Breaches.None(found);
    }

    static bool NamesNamespace(string item, string ns) =>
        item == ns || item.StartsWith(ns + ".", StringComparison.Ordinal);

    static bool HoldsOnlyUsings(SourceFile file)
    {
        var lines = file.Lines
            .Select(line => line.Trim())
            .Where(line => line.Length > 0 && !line.StartsWith("//", StringComparison.Ordinal))
            .ToList();
        return lines.Count > 0 && lines.All(line =>
            line.StartsWith("using ", StringComparison.Ordinal) || line.StartsWith("global using ", StringComparison.Ordinal));
    }
}
