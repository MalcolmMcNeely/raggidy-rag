namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class Repo
{
    readonly List<string> skipFolders;
    readonly List<string> extensions;
    readonly List<Regex> testPatterns;
    readonly List<CodeRoot> codeRoots = [];
    readonly List<SourceFile> sourceFiles = [];
    readonly List<string> folders = [];

    Repo(string root)
    {
        Root = root;
        var placement = Rule("file-placement");
        skipFolders = placement.Strings("skip-folders").ToList();
        extensions = placement.Strings("source-files").ToList();
        testPatterns = placement.Strings("test-files").Select(Glob).ToList();
        Walk(root, "");
        if (sourceFiles.Count == 0)
        {
            throw new InvalidOperationException("the walk from " + root + " found no source file, so nothing was checked");
        }

        var thisProject = typeof(Repo).Assembly.GetName().Name;
        ThisProject = codeRoots.Single(codeRoot => codeRoot.ProjectName == thisProject);
    }

    public string Root { get; }

    public CodeRoot ThisProject { get; }

    public IReadOnlyList<string> SkipFolders => skipFolders;

    public IReadOnlyList<CodeRoot> CodeRoots => codeRoots;

    public IReadOnlyList<SourceFile> SourceFiles => sourceFiles;

    public IReadOnlyList<string> Folders => folders;

    public static Repo Find()
    {
        for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
        {
            if (Directory.Exists(Path.Combine(folder.FullName, "docs", "agents", "rules")))
            {
                return new Repo(folder.FullName);
            }
        }

        throw new InvalidOperationException("no docs/agents/rules folder above " + AppContext.BaseDirectory);
    }

    public Rule Rule(string name) => Reading.Rule.Read(Path.Combine(Root, "docs", "agents", "rules", name + ".md"));

    public bool Exists(string relative) => File.Exists(Path.Combine(Root, relative));

    public string TextOf(string relative) => File.ReadAllText(Path.Combine(Root, relative));

    public IEnumerable<string> FoldersHoldingSource() =>
        sourceFiles.SelectMany(file => Ancestors(file.Folder)).Distinct();

    public static IEnumerable<string> Ancestors(string folder)
    {
        for (var current = folder; current.Length > 0; current = ParentOf(current))
        {
            yield return current;
        }
    }

    public static string ParentOf(string folder)
    {
        var slash = folder.LastIndexOf('/');
        return slash < 0 ? "" : folder[..slash];
    }

    public static string NameOf(string folder) => folder[(folder.LastIndexOf('/') + 1)..];

    public static Regex Glob(string pattern) =>
        new("^" + Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal) + "$");

    void Walk(string folder, string relative)
    {
        var codeRoot = CodeRoot.At(folder, relative);
        if (codeRoot is not null)
        {
            codeRoots.Add(codeRoot);
        }

        foreach (var sub in Directory.EnumerateDirectories(folder).Order(StringComparer.Ordinal))
        {
            var name = Path.GetFileName(sub);
            if (name == ".git" || IsSkipped(name) || IsAnotherCheckout(sub))
            {
                continue;
            }

            var subRelative = relative.Length == 0 ? name : relative + "/" + name;
            folders.Add(subRelative);
            Walk(sub, subRelative);
        }

        foreach (var file in Directory.EnumerateFiles(folder).Order(StringComparer.Ordinal))
        {
            if (!extensions.Contains(Path.GetExtension(file), StringComparer.Ordinal))
            {
                continue;
            }

            var name = Path.GetFileName(file);
            var fileRelative = relative.Length == 0 ? name : relative + "/" + name;
            var isTest = testPatterns.Any(pattern => pattern.IsMatch(name));
            sourceFiles.Add(new SourceFile(file, fileRelative, isTest, NearestCodeRoot(fileRelative)));
        }
    }

    CodeRoot? NearestCodeRoot(string relative) =>
        codeRoots.Where(codeRoot => codeRoot.Holds(relative)).MaxBy(codeRoot => codeRoot.Folder.Length);

    bool IsSkipped(string name) => skipFolders.Contains(name, StringComparer.OrdinalIgnoreCase);

    static bool IsAnotherCheckout(string folder) =>
        Directory.Exists(Path.Combine(folder, ".git")) || File.Exists(Path.Combine(folder, ".git"));
}
