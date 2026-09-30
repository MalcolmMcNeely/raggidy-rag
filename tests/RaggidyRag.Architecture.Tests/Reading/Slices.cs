namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class Slices
{
    Slices(IReadOnlyList<string> names, IReadOnlyList<string> concerns, IReadOnlyList<(CodeRoot Root, Context Context)> roots)
    {
        Names = names;
        Concerns = concerns;
        Roots = roots;
    }

    public IReadOnlyList<string> Names { get; }

    public IReadOnlyList<string> Concerns { get; }

    public IReadOnlyList<(CodeRoot Root, Context Context)> Roots { get; }

    public bool ReachAnyCode => Roots.Count > 0;

    public string NotReached => "no context declares Slices, so the rule reaches no code";

    // This project proves the repo and holds no Slice, so the eight Slice rules leave it out.
    public static Slices Read(Repo repo)
    {
        var placement = repo.Rule("file-placement");
        var map = ContextMap.Read(repo);
        var roots = repo.CodeRoots
            .Where(codeRoot => codeRoot != repo.ThisProject)
            .Select(codeRoot => (Root: codeRoot, Context: map.Claiming(codeRoot.Folder)))
            .Where(pair => pair.Context is { DeclaresSlices: true })
            .Select(pair => (pair.Root, pair.Context!))
            .ToList();
        return new Slices(placement.Strings("slices"), placement.Strings("concerns"), roots);
    }

    public string Expected(CodeRoot root, string slice) => root.IsFrontEnd ? slice.ToLowerInvariant() : slice;

    public string SharedName(CodeRoot root) => root.IsFrontEnd ? "shared" : "Shared";

    public bool IsSlice(CodeRoot root, string folderName) => Names.Any(slice => Expected(root, slice) == folderName);

    public bool IsShared(CodeRoot root, string folderName) => SharedName(root) == folderName;

    public IEnumerable<SourceFile> FilesOf(Repo repo, CodeRoot root) =>
        repo.SourceFiles.Where(file => file.CodeRoot == root);

    public IEnumerable<string> FoldersWithSource(Repo repo, CodeRoot root) =>
        FilesOf(repo, root).SelectMany(file => Repo.Ancestors(Repo.ParentOf(file.PathInCodeRoot))).Distinct();

    public static int DepthOf(string folder) => folder.Count(mark => mark == '/') + 1;
}
