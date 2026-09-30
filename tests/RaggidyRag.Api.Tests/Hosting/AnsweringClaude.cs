using System.Collections.Concurrent;
using Microsoft.Extensions.AI;

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class AnsweringClaude(string answer) : IChatClient
{
    readonly ConcurrentQueue<string> prompts = new();

    public IReadOnlyCollection<string> Prompts => prompts;

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        prompts.Enqueue(string.Concat(messages.Select(message => message.Text)));
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, answer)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Ask takes the Answer whole.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
