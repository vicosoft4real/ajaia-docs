using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Infrastructure.Data;
using AjaiaDocs.Infrastructure.Data.Migrations;
using AjaiaDocs.Infrastructure.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AjaiaDocs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAjaiaDocsInfrastructure(this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:Postgres' is required.");

        services.AddSingleton(_ => new AjaiaDbConnectionFactory(connectionString));
        services.AddSingleton<SchemaMigrationRunner>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
