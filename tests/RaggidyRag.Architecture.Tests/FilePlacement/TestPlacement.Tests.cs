namespace RaggidyRag.Architecture.Tests.FilePlacement;

public sealed class TestPlacementTests
{
    // The starter tests prove the repo and test no code file, so they are left out.
    [Fact]
    public void EveryTestSitsWhereTheCodeItTestsSits()
    {
        var repo = Repo.Find();
        var testRoots = repo.Rule("file-placement").Map("test-roots");
        var codeFiles = repo.SourceFiles.Where(file => !file.IsTest).ToList();

        var found = repo.SourceFiles
            .Where(file => file.IsTest && file.CodeRoot != repo.ThisProject)
            .Select(test => Misplaced(repo, test, codeFiles, testRoots))
            .OfType<string>()
            .ToList();

        Breaches.None(found);
    }

    static string? Misplaced(Repo repo, SourceFile test, List<SourceFile> codeFiles, IReadOnlyDictionary<string, string> testRoots)
    {
        if (test.Extension == ".cs")
        {
            return MisplacedInProject(repo, test, codeFiles);
        }

        if (Beside(test, test.Folder, codeFiles))
        {
            return null;
        }

        foreach (var (testRoot, mirrored) in testRoots)
        {
            var root = testRoot.Trim('/');
            if (test.Folder != root && !test.Folder.StartsWith(root + "/", StringComparison.Ordinal))
            {
                continue;
            }

            var within = test.Folder[root.Length..].TrimStart('/');
            var expected = (mirrored.Trim('/') + "/" + within).Trim('/');
            return Beside(test, expected, codeFiles) ? null : $"{test} tests {test.Subject}, and {expected}/ holds no {test.Subject}";
        }

        return $"{test} tests {test.Subject}, and no {test.Subject} sits beside it";
    }

    static string? MisplacedInProject(Repo repo, SourceFile test, List<SourceFile> codeFiles)
    {
        if (test.CodeRoot is not { IsTestProject: true })
        {
            return $"{test} is a C# test outside a .Tests project";
        }

        var codeProject = test.CodeRoot.ProjectName[..^".Tests".Length];
        var codeRoot = repo.CodeRoots.FirstOrDefault(root => root.ProjectName == codeProject);
        if (codeRoot is null)
        {
            return $"{test} is in {test.CodeRoot.ProjectName}, and there is no project {codeProject}";
        }

        var folder = Repo.ParentOf(test.PathInCodeRoot);
        var mirrored = codeFiles.Any(code =>
            code.CodeRoot == codeRoot && Repo.ParentOf(code.PathInCodeRoot) == folder && code.Subject == test.Subject);
        return mirrored ? null : $"{test} tests {test.Subject}, and {codeRoot.Folder}/{folder} holds no {test.Subject}";
    }

    static bool Beside(SourceFile test, string folder, List<SourceFile> codeFiles) =>
        codeFiles.Any(code => code.Folder == folder && code.Subject == test.Subject);
}
