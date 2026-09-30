using System.Text;

namespace RaggidyRag.Api.Asking;

public sealed class PromptBuilder
{
    // A raw string takes the line endings of the checkout, so the Prompt would differ from one OS to the next.
    static readonly string Instructions = """
        Answer the Question from the Chunks below, and from nothing else.
        After each fact, put the number of the Chunk that supports it in square brackets, such as [2].
        When the Chunks do not answer the Question, say that the documents do not say.
        """.ReplaceLineEndings("\n");

    public string Build(Question question, IReadOnlyList<RetrievedChunk> chunks)
    {
        var prompt = new StringBuilder(Instructions).Append("\n\n");
        foreach (var chunk in chunks)
        {
            prompt.Append($"Chunk [{chunk.Number}]\n")
                .Append($"Document: {chunk.DocumentPath}\n")
                .Append($"Heading trail: {chunk.HeadingTrail}\n")
                .Append($"{chunk.Text}\n\n");
        }

        return prompt.Append($"Question: {question.Text}").ToString();
    }
}
