namespace RaggidyRag.Architecture.Tests.Reading;

public sealed record Context(string Name, string Folder, Glossary Glossary, bool DeclaresSlices)
{
    public bool Claims(string relativePath) =>
        Folder.Length == 0 || relativePath == Folder || relativePath.StartsWith(Folder + "/", StringComparison.Ordinal);
}
