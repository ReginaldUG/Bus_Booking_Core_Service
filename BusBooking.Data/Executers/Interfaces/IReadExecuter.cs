namespace BusBooking.Data.Executers.Interfaces;

public interface IReadExecuter
{
    IEnumerable<T> ExecuteReader<T>(string? connStr, string query, object? param);
    Task<IEnumerable<T>> ExecuteReaderAsync<T>(string? connStr, string query, object? param);
    
}