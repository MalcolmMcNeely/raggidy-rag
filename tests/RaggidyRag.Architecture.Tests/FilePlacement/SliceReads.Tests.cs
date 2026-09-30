namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class SliceReadsTests
{
    static readonly Regex Import = new(@"(?:from|import|require\()\s*['""]([^'""]+)['""]");

    [Fact]
    public void ASliceNeverReadsAnotherSlice()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);

        var found = new List<string>();
        foreach (var (root, _) in slices.Roots)
        {
            foreach (var file in slices.FilesOf(repo, root).Where(file => !file.IsTest))
            {
                var own = file.FirstFolderInCodeRoot;
                if (own is null || !slices.IsSlice(root, own))
                {
                    continue;
                }

                found.AddRange(ReadsOf(slices, root, file)
                    .Where(read => read != own)
                    .Select(read => $"{file} in the Slice {own} reads the Slice {read}"));
            }
        }

        Breaches.None(found);
    }

    [Fact]
    public void SharedNeverReadsASlice()
    {
        var repo = Repo.Find();
        var slices = Slices.Read(repo);
        Assert.SkipWhen(!slices.ReachAnyCode, slices.NotReached);

        var found = new List<string>();
        foreach (var (root, _) in slices.Roots)
        {
            foreach (var file in slices.FilesOf(repo, root).Where(file => !file.IsTest))
            {
                var own = file.FirstFolderInCodeRoot;
                if (own is null || !slices.IsShared(root, own))
                {
                    continue;
                }

                found.AddRange(ReadsOf(slices, root, file).Select(read => $"{file} in {own} reads the Slice {read}"));
            }
        }

        Breaches.None(found);
    }

    // A C# file names a Slice by its namespace. A front-end file names one by a relative import.
    static IEnumerable<string> ReadsOf(Slices slices, CodeRoot root, SourceFile file)
    {
        if (root.IsFrontEnd)
        {
            return Import.Matches(file.Text)
                .Select(match => match.Groups[1].Value)
                .Where(spec => spec.StartsWith('.'))
                .Select(spec => Normalized(Repo.ParentOf(file.PathInCodeRoot) + "/" + spec).Split('/')[0])
                .Where(first => slices.IsSlice(root, first))
                .Distinct();
        }

        return slices.Names.Where(slice =>
            Regex.IsMatch(file.Text, @"(?<![\w.])" + Regex.Escape(root.RootNamespace + "." + slice) + @"(?!\w)"));
    }

    static string Normalized(string path)
    {
        var parts = new List<string>();
        foreach (var part in path.Split('/'))
        {
            if (part.Length == 0 || part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (parts.Count > 0)
                {
                    parts.RemoveAt(parts.Count - 1);
                }

                continue;
            }

            parts.Add(part);
        }

        return string.Join("/", parts);
    }
}
