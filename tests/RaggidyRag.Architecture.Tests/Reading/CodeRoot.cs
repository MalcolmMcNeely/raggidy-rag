using System.Xml.Linq;

namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class CodeRoot
{
    CodeRoot(string folder, string projectName, string rootNamespace, bool isFrontEnd)
    {
        Folder = folder;
        ProjectName = projectName;
        RootNamespace = rootNamespace;
        IsFrontEnd = isFrontEnd;
    }

    public string Folder { get; }

    public string ProjectName { get; }

    public string RootNamespace { get; }

    public bool IsFrontEnd { get; }

    public bool IsTestProject => ProjectName.EndsWith(".Tests", StringComparison.Ordinal);

    public static CodeRoot? At(string folder, string relative)
    {
        var project = Directory.EnumerateFiles(folder, "*.csproj").FirstOrDefault();
        if (project is not null)
        {
            var name = Path.GetFileNameWithoutExtension(project);
            var declared = XDocument.Load(project).Descendants("RootNamespace").FirstOrDefault()?.Value.Trim();
            return new CodeRoot(relative, name, string.IsNullOrEmpty(declared) ? name : declared, false);
        }

        var parent = Path.GetDirectoryName(folder);
        if (Path.GetFileName(folder) == "src" && parent is not null && File.Exists(Path.Combine(parent, "package.json")))
        {
            return new CodeRoot(relative, Path.GetFileName(parent), "", true);
        }

        return null;
    }

    public IReadOnlyList<string> GlobalUsings(string repoRoot)
    {
        var project = Directory.EnumerateFiles(Path.Combine(repoRoot, Folder), "*.csproj").FirstOrDefault();
        return project is null
            ? []
            : XDocument.Load(project).Descendants("Using").Select(item => (string?)item.Attribute("Include") ?? "").ToList();
    }

    public bool Holds(string relativePath) =>
        Folder.Length == 0 || relativePath.StartsWith(Folder + "/", StringComparison.Ordinal);

    public string PathWithin(string relativePath) => Folder.Length == 0 ? relativePath : relativePath[(Folder.Length + 1)..];

    public override string ToString() => Folder;
}
