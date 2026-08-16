using Npgsql;

namespace AjaiaDocs.Infrastructure.Data;

public static class PostgresConnectionString
{
    public static string Normalize(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var candidate = connectionString.Trim();
        var schemeSeparator = candidate.IndexOf("://", StringComparison.Ordinal);
        var assignment = candidate.IndexOf('=');
        if (schemeSeparator < 0 || assignment >= 0 && assignment < schemeSeparator)
        {
            if (candidate.StartsWith("postgresql:", StringComparison.OrdinalIgnoreCase) ||
                candidate.StartsWith("postgres:", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("The PostgreSQL connection URI is malformed.",
                    nameof(connectionString));
            }

            return connectionString;
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            !uri.Scheme.Equals("postgresql", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(uri.Host) || uri.Port <= 0 ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException("The PostgreSQL connection URI is malformed or unsupported.",
                nameof(connectionString));
        }

        var separator = uri.UserInfo.IndexOf(':');
        if (separator <= 0 || separator == uri.UserInfo.Length - 1)
        {
            throw new ArgumentException("The PostgreSQL connection URI must include a username and password.",
                nameof(connectionString));
        }

        var databasePath = uri.AbsolutePath;
        if (databasePath.Length <= 1 || databasePath[1..].Contains('/'))
        {
            throw new ArgumentException("The PostgreSQL connection URI must include one database name.",
                nameof(connectionString));
        }

        var username = Decode(uri.UserInfo[..separator], connectionString);
        var password = Decode(uri.UserInfo[(separator + 1)..], connectionString);
        var database = Decode(databasePath[1..], connectionString);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password) ||
            string.IsNullOrWhiteSpace(database))
        {
            throw new ArgumentException("The PostgreSQL connection URI contains an empty credential or database name.",
                nameof(connectionString));
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port,
            Username = username,
            Password = password,
            Database = database
        }.ConnectionString;
    }

    private static string Decode(string value, string connectionString)
    {
        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] == '%' &&
                (index + 2 >= value.Length || !Uri.IsHexDigit(value[index + 1]) ||
                 !Uri.IsHexDigit(value[index + 2])))
            {
                throw new ArgumentException("The PostgreSQL connection URI contains invalid escaping.",
                    nameof(connectionString));
            }
        }

        return Uri.UnescapeDataString(value);
    }
}
