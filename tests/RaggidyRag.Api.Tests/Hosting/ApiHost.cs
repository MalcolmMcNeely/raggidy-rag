using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace RaggidyRag.Api.Tests.Hosting;

public sealed class ApiHost(Postgres postgres) : WebApplicationFactory<Program>
{
    public FakeTimeProvider Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:raggidyrag", postgres.ConnectionString);
        builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(Clock));
    }
}
