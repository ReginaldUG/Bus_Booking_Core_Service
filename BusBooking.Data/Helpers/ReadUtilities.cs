using System.Text;
using BusBooking.Data.Extensions;
using Microsoft.Extensions.Configuration;

namespace BusBooking.Data.Helpers;

public class ReadUtilities
{
    private readonly IConfiguration _configuration;

    public ReadUtilities(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetConnectionString() => _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;

    public string GenerateSelectCountInAsync<TEntity>(KeyValuePair<string, List<string>> criteria)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        var selectQuery = new StringBuilder($"SELECT COUNT(1) FROM {tableName} WHERE \"{criteria.Key}\" in (");
        foreach (var itemValue in criteria.Value) selectQuery.Append($"'{itemValue}',");
        return $"{selectQuery.ToString().Substring(0, selectQuery.Length - 1)})";
    }

    public string GenerateSelectQuery<TEntity>(int pageSize, int pageNumber)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        return $"SELECT * FROM {tableName} ORDER BY \"Id\" DESC LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";
    }

    public string GenerateSelectSingleRecordQuery<TEntity>(string propertyName, string value)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        return $"SELECT * FROM {tableName} WHERE \"{propertyName}\" = '{value.Trim()}' LIMIT 1";
    }
}