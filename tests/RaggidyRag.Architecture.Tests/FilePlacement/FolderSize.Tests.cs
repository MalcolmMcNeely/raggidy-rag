namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class FolderSizeTests
{
    [Fact]
    public void NoFolderHoldsMoreSubjectsThanTheLimit()
    {
        var repo = Repo.Find();
        var limit = repo.Rule("file-placement").Int("max-types-per-folder");

        var found = repo.SourceFiles
            .GroupBy(file => file.Folder)
            .Select(folder => (Folder: folder.Key, Subjects: folder.Select(file => file.Subject).Distinct(StringComparer.Ordinal).Count()))
            .Where(folder => folder.Subjects > limit)
            .Select(folder => $"{folder.Folder}/ holds {folder.Subjects} subjects, and max-types-per-folder is {limit}")
            .ToList();

        Breaches.None(found);
    }
}
