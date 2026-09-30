namespace RaggidyRag.Api.Failures;

public sealed class ServiceFailed(string service, Exception failure) : Exception($"{service} failed: {failure.Message}", failure)
{
    public const string Voyage = "Voyage";
    public const string Claude = "Claude";
    public const string Postgres = "Postgres";

    public string Service { get; } = service;

    // A request the caller gave up on is no service's fault, so its cancellation passes through unnamed.
    public static async Task<T> Blame<T>(string service, Func<Task<T>> call, CancellationToken cancellation)
    {
        try
        {
            return await call();
        }
        catch (Exception failure) when (!cancellation.IsCancellationRequested)
        {
            throw new ServiceFailed(service, failure);
        }
    }

    public static Task Blame(string service, Func<Task> call, CancellationToken cancellation) =>
        Blame(service, async () =>
        {
            await call();
            return true;
        }, cancellation);
}
