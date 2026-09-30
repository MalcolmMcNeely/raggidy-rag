using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class ApiHost(Postgres postgres) : WebApplicationFactory<Program>, IAsyncLifetime
{
    string connectionString = "";

    public FakeTimeProvider Clock { get; } = new();

    public FixedEmbeddings Embeddings { get; } = new();

    public async ValueTask InitializeAsync() => connectionString = await postgres.CreateDatabase();

    public WebApplicationFactory<Program> WithDocuments(string folder) =>
        WithWebHostBuilder(builder => builder.UseSetting("Ingest:DocumentsFolder", Path.Combine(AppContext.BaseDirectory, "Documents", folder)));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:raggidyrag", connectionString);
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<TimeProvider>(Clock);
            services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(Embeddings);
        });
    }
}
