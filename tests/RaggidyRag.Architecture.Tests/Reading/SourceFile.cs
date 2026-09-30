namespace RaggidyRag.Architecture.Tests.Reading;

public sealed class SourceFile
{
    string? text;

    public SourceFile(string path, string relative, bool isTest, CodeRoot? codeRoot)
    {
        Path = path;
        Relative = relative;
        IsTest = isTest;
        CodeRoot = codeRoot;
        Name = System.IO.Path.GetFileName(path);
        Extension = System.IO.Path.GetExtension(path);
        var dot = Name.IndexOf('.');
        Subject = dot < 0 ? Name : Name[..dot];
        var slash = relative.LastIndexOf('/');
        Folder = slash < 0 ? "" : relative[..slash];
    }

    public string Path { get; }

    public string Relative { get; }

    public string Folder { get; }

    public string Name { get; }

    public string Extension { get; }

    public string Subject { get; }

    public bool IsTest { get; }

    public CodeRoot? CodeRoot { get; }

    public string Text => text ??= File.ReadAllText(Path);

    public IEnumerable<string> Lines => Text.Split('\n').Select(line => line.TrimEnd('\r'));

    public string PathInCodeRoot => CodeRoot?.PathWithin(Relative) ?? Relative;

    public string? FirstFolderInCodeRoot
    {
        get
        {
            var within = PathInCodeRoot;
            var slash = within.IndexOf('/');
            return slash < 0 ? null : within[..slash];
        }
    }

    public override string ToString() => Relative;
}
