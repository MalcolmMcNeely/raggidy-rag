using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunks;

namespace RaggidyRag.Api.Asking;

public sealed partial class Ask(
    ChunkStore chunks,
    Retrieve retrieve,
    PromptBuilder prompts,
    IChatClient claude,
    IOptions<ClaudeOptions> options,
    TimeProvider clock)
{
    public const string NothingIngestedYet = "Nothing has been ingested yet. Press Ingest first.";

    public async Task<AskResult> Answer(Question question, CancellationToken cancellation)
    {
        if (!await chunks.HoldsAnyChunk(cancellation))
        {
            return new AskResult(NothingIngestedYet, [], []);
        }

        var retrieved = await retrieve.Run(question, cancellation);
        var answer = await Write(prompts.Build(question, retrieved), cancellation);
        return new AskResult(answer, Cited(answer, retrieved), retrieved);
    }

    static List<Citation> Cited(string answer, IReadOnlyList<RetrievedChunk> retrieved)
    {
        var used = Mark().Matches(answer).Select(mark => int.Parse(mark.Groups[1].Value, CultureInfo.InvariantCulture)).ToHashSet();
        return retrieved
            .Where(chunk => used.Contains(chunk.Number))
            .Select(chunk => new Citation(chunk.Number, chunk.DocumentPath, chunk.HeadingTrail))
            .ToList();
    }

    [GeneratedRegex(@"\[(\d+)\]")]
    private static partial Regex Mark();

    async Task<string> Write(string prompt, CancellationToken cancellation)
    {
        using var patience = new CancellationTokenSource(options.Value.Patience, clock);
        using var either = CancellationTokenSource.CreateLinkedTokenSource(cancellation, patience.Token);
        var written = await claude.GetResponseAsync(
            prompt, new ChatOptions { MaxOutputTokens = options.Value.MaxTokens }, either.Token);
        return written.Text;
    }
}
