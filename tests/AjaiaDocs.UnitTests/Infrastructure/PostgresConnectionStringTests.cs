using AjaiaDocs.Infrastructure.Data;
using Npgsql;

namespace AjaiaDocs.UnitTests.Infrastructure;

public sealed class PostgresConnectionStringTests
{
    [Fact]
    public void Normalize_PreservesNpgsqlKeywordConnectionStrings()
    {
        const string connectionString = "Host=localhost;Port=5432;Database=ajaia;Username=user;Password=https://secret.example";

        Assert.Same(connectionString, PostgresConnectionString.Normalize(connectionString));
    }

    [Fact]
    public void Normalize_ConvertsAndDecodesRenderPostgresUri()
    {
        var normalized = PostgresConnectionString.Normalize(
            "postgresql://render%40user:p%40ss%3Aword@internal-db:5432/ajaia%2Ddocs");
        var parsed = new NpgsqlConnectionStringBuilder(normalized);

        Assert.Equal("internal-db", parsed.Host);
        Assert.Equal(5432, parsed.Port);
        Assert.Equal("render@user", parsed.Username);
        Assert.Equal("p@ss:word", parsed.Password);
        Assert.Equal("ajaia-docs", parsed.Database);
    }

    [Fact]
    public void Normalize_UsesPostgresDefaultPortWhenRenderUriOmitsPort()
    {
        var normalized = PostgresConnectionString.Normalize(
            "postgresql://render:secret@internal-db/ajaia_docs");
        var parsed = new NpgsqlConnectionStringBuilder(normalized);

        Assert.Equal("internal-db", parsed.Host);
        Assert.Equal(5432, parsed.Port);
        Assert.Equal("ajaia_docs", parsed.Database);
    }

    [Theory]
    [InlineData("postgresql://user@host:5432/database")]
    [InlineData("postgresql://user:password@:5432/database")]
    [InlineData("postgresql://user:password@host:0/database")]
    [InlineData("postgresql://user:password@host:5432/")]
    [InlineData("postgresql://user:password@host:5432/database?sslmode=require")]
    [InlineData("mysql://user:password@host:3306/database")]
    public void Normalize_RejectsMalformedOrUnsupportedUris(string connectionString)
    {
        Assert.Throws<ArgumentException>(() => PostgresConnectionString.Normalize(connectionString));
    }
}
