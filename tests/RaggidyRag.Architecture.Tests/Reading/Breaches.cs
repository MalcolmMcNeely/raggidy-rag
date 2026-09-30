namespace RaggidyRag.Architecture.Tests.Reading;

public static class Breaches
{
    public static void None(IReadOnlyCollection<string> found) =>
        Assert.True(found.Count == 0, found.Count + " breach(es):\n" + string.Join("\n", found));
}
