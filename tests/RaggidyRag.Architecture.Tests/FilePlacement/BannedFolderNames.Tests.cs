namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class BannedFolderNamesTests
{
    [Fact]
    public void NoFolderCarriesABannedName()
    {
        var repo = Repo.Find();
        var banned = repo.Rule("file-placement").Strings("banned-folder-names");

        var found = repo.FoldersHoldingSource()
            .Where(folder => banned.Contains(Repo.NameOf(folder), StringComparer.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .Select(folder => $"{folder}/ is named for a word in banned-folder-names")
            .ToList();

        Breaches.None(found);
    }
}
