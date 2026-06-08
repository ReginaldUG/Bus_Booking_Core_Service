using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BusBooking.Data.Executers;

public class WriteExecuter
{
    private readonly int _timeout;

    public WriteExecuter(IConfiguration configuration)
    {
        _timeout = int.Parse(configuration.GetSection("AppSettings")["DatabaseWriteTimeout"] ?? "30");        
    }

    public async Task ExecuteCommandAsync(string? connStr, string query, object param)
    {
        await using var conn = new NpgsqlConnection(connStr);
        await conn.ExecuteAsync(query, param, commandTimeout: _timeout);
    }

    public async Task ExecuteCommandAsync(string query, object param, NpgsqlTransaction transaction)
    {
        if (transaction == null) 
            throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");

        await transaction.Connection.ExecuteAsync(query, param, transaction, commandTimeout: _timeout);
    }
}