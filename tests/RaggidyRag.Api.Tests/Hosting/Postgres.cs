using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(RaggidyRag.Api.Tests.Hosting.Postgres))]

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class Postgres : IAsyncLifetime
{
    readonly PostgreSqlContainer container = Podman.Host(new PostgreSqlBuilder())
        .WithImage("pgvector/pgvector:pg17")
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public ValueTask InitializeAsync() => new(container.StartAsync());

    public ValueTask DisposeAsync() => container.DisposeAsync();
}
