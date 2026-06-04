using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;


namespace BusBooking.Data.Executers;

public class ReadExecuter
{
    private readonly int _timeout;
    public ReadExecuter(IConfiguration configuration)
    {
        _timeout = int.Parse(configuration.GetSection("AppSettings")["DatabaseReadTimeout"] ?? "30");
    }
    
    public IEnumerable<T> ExecuteReader<T>(string? connStr, string query, object? param)
    {
        using var conn = new NpgsqlConnection(connStr);
        var response = conn.Query<T>(query, param, commandTimeout: _timeout) ?? Array.Empty<T>();
        return response;
    }

    public async Task<IEnumerable<T>> ExecuteReaderAsync<T>(string? connStr, string query, object? param)
    {
        await using var conn = new NpgsqlConnection(connStr);
        var response = await conn.QueryAsync<T>(query, param, commandTimeout: _timeout);
        return response;
    }
}