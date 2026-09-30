using Microsoft.Extensions.AI;

namespace RaggidyRag.Api.Embedding;

// Voyage tells a document apart from a query by input_type, and embeds each a little differently.
public static class InputType
{
    public const string Key = "input_type";

    public static EmbeddingGenerationOptions Document { get; } = new() { AdditionalProperties = new() { [Key] = "document" } };
}
