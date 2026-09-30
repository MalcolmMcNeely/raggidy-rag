using Microsoft.Extensions.AI;

namespace RaggidyRag.Api.Tests.Hosting;

// Holding is the fact a test waits on before it acts, so the act is never a race.
public sealed class HoldingClaude : IChatClient
{
    readonly TaskCompletionSource holding = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Holding => holding.Task;

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        holding.TrySetResult();
        var answer = new TaskCompletionSource<ChatResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var letGo = cancellationToken.Register(() => answer.TrySetCanceled(cancellationToken));
        return await answer.Task;
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Ask takes the Answer whole.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
