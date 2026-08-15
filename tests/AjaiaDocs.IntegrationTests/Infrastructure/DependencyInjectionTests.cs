using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Infrastructure;
using AjaiaDocs.Infrastructure.Data;
using AjaiaDocs.Infrastructure.Data.Migrations;
using AjaiaDocs.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjaiaDocs.IntegrationTests.Infrastructure;

[Collection(PostgresCollection.CollectionName)]
public sealed class DependencyInjectionTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Infrastructure_registrations_have_expected_lifetimes_and_provider_owns_data_source()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = fixture.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddAjaiaDocsInfrastructure(configuration);
        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<AjaiaDbConnectionFactory>();

        try
        {
            Assert.Same(factory, provider.GetRequiredService<AjaiaDbConnectionFactory>());
            Assert.NotNull(provider.GetRequiredService<SchemaMigrationRunner>());

            using var scope = provider.CreateScope();
            Assert.IsType<DocumentRepository>(scope.ServiceProvider
                .GetRequiredService<IDocumentRepository>());
            Assert.IsType<UserRepository>(scope.ServiceProvider
                .GetRequiredService<IUserRepository>());

            await provider.DisposeAsync();

            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                factory.OpenConnectionAsync(CancellationToken.None));
        }
        finally
        {
            await factory.DisposeAsync();
        }
    }
}
