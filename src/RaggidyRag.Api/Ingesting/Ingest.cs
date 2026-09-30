using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using RaggidyRag.Api.Chunking;
using RaggidyRag.Api.Chunks;
using RaggidyRag.Api.Embedding;
using RaggidyRag.Api.Failures;

namespace RaggidyRag.Api.Ingesting;

public sealed class Ingest(
    IOptions<IngestOptions> options,
    IHostEnvironment environment,
    Chunker chunker,
    IEmbeddingGenerator<string, Embedding<float>> embeddings,
    ChunkStore store)
{
    public async Task<IngestResult> Run(CancellationToken cancellation)
    {
        var folder = Path.GetFullPath(options.Value.DocumentsFolder, environment.ContentRootPath);
        var documents = await ReadDocuments(folder, cancellation);
        var chunks = documents.SelectMany(chunker.Cut).ToList();

        var vectors = await ServiceFailed.Blame(
            ServiceFailed.Voyage, () => embeddings.GenerateAsync(chunks.Select(chunk => chunk.Text), InputType.Document, cancellation), cancellation);
        foreach (var (chunk, vector) in chunks.Zip(vectors))
        {
            chunk.Embedding = vector.Vector;
        }

        foreach (var document in documents)
        {
            await store.Replace(document.Path, chunks.Where(chunk => chunk.DocumentPath == document.Path), cancellation);
        }

        return new IngestResult(documents.Count, chunks.Count);
    }

    static async Task<List<Document>> ReadDocuments(string folder, CancellationToken cancellation)
    {
        var documents = new List<Document>();
        var markdown = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            .Where(path => Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal);
        foreach (var path in markdown)
        {
            // The path is the Document's name in every Citation and Chunk id, so it reads the same on every OS.
            var relative = Path.GetRelativePath(folder, path).Replace('\\', '/');
            documents.Add(new Document(relative, await File.ReadAllTextAsync(path, cancellation)));
        }

        return documents;
    }
}
