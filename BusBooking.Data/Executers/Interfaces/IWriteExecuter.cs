using Npgsql;

namespace BusBooking.Data.Executers.Interfaces;

public interface IWriteExecuter
{
    Task ExecuteCommandAsync(string? connStr, string query, object param);
    Task ExecuteCommandAsync(string query, object param, NpgsqlTransaction transaction);
    Task<int> ExecuteCommandAndReturnIdAsync(string query, object param, NpgsqlTransaction transaction);
}