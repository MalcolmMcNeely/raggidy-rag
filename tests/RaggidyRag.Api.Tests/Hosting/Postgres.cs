using Npgsql;
using Testcontainers.PostgreSql;

[assembly: AssemblyFixture(typeof(RaggidyRag.Api.Tests.Hosting.Postgres))]

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class Postgres : IAsyncLifetime
{
    static int databases;

    readonly PostgreSqlContainer container = Podman.Host(new PostgreSqlBuilder())
        .WithImage("pgvector/pgvector:pg17")
        .Build();

    public ValueTask InitializeAsync() => new(container.StartAsync());

    public ValueTask DisposeAsync() => container.DisposeAsync();

    // Each test host gets a database of its own, so what one class ingests never shows in another.
    public async Task<string> CreateDatabase()
    {
        var name = $"raggidyrag_{Interlocked.Increment(ref databases)}";
        var created = await container.ExecScriptAsync($"CREATE DATABASE \"{name}\";");
        if (created.ExitCode != 0)
        {
            throw new InvalidOperationException($"Postgres did not create {name}: {created.Stderr}");
        }

        return new NpgsqlConnectionStringBuilder(container.GetConnectionString()) { Database = name }.ConnectionString;
    }
}
