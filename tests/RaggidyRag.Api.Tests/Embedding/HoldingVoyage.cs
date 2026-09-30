namespace RaggidyRag.Api.Tests.Embedding;

// Holding is the fact a test waits on before it acts, so the act is never a race.
public sealed class HoldingVoyage : HttpMessageHandler
{
    readonly TaskCompletionSource holding = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Holding => holding.Task;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        holding.TrySetResult();
        var answer = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var letGo = cancellationToken.Register(() => answer.TrySetCanceled(cancellationToken));
        return await answer.Task;
    }
}
