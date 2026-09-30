using Testcontainers.PostgreSql;

namespace RaggidyRag.Api.Tests.Hosting;

public static class Podman
{
    // On Windows, Testcontainers looks only for Docker's pipe, and a Podman machine listens on its own.
    public static PostgreSqlBuilder Host(PostgreSqlBuilder builder) =>
        OperatingSystem.IsWindows() ? builder.WithDockerEndpoint("npipe://./pipe/podman-machine-default") : builder;
}
