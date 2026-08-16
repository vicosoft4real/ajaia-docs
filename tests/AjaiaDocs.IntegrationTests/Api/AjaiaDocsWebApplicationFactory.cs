using AjaiaDocs.IntegrationTests.Infrastructure;
using AjaiaDocs.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AjaiaDocs.IntegrationTests.Api;

public sealed class AjaiaDocsWebApplicationFactory(PostgresFixture postgres,
    string environment = "Development")
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = postgres.ConnectionString
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<AjaiaDbConnectionFactory>();
            services.AddSingleton(_ => new AjaiaDbConnectionFactory(postgres.ConnectionString));
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(Guid userId)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
        var response = await client.SendWithAntiforgeryAsync(HttpMethod.Post,
            "/api/session", new { userId });
        response.EnsureSuccessStatusCode();
        return client;
    }
}
