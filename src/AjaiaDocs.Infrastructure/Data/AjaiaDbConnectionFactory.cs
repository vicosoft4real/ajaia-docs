using Npgsql;

namespace AjaiaDocs.Infrastructure.Data;

public sealed class AjaiaDbConnectionFactory : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public AjaiaDbConnectionFactory(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _dataSource = NpgsqlDataSource.Create(PostgresConnectionString.Normalize(connectionString));
    }

    public async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken ct) =>
        await _dataSource.OpenConnectionAsync(ct);

    public ValueTask DisposeAsync() => _dataSource.DisposeAsync();
}
