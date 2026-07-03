using System.Text;
using BusBooking.Data.Extensions;
using BusBooking.Data.Helpers.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BusBooking.Data.Helpers.Implementations;

public class ReadUtilities : IReadUtilities
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
        var selectQuery = new StringBuilder($"SELECT COUNT(1) FROM \"{tableName}\" WHERE \"{criteria.Key}\" in (");
        foreach (var itemValue in criteria.Value) selectQuery.Append($"'{itemValue}',");
        return $"{selectQuery.ToString().Substring(0, selectQuery.Length - 1)})";
    }

    public string GenerateSelectQuery<TEntity>(int pageSize, int pageNumber)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        return $"SELECT * FROM \"{tableName}\" ORDER BY \"Id\" DESC LIMIT {pageSize} OFFSET {(pageNumber - 1) * pageSize}";
    }

    public string GenerateSelectSingleRecordQuery<TEntity>(string propertyName, string value)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" = '{value.Trim()}' ORDER BY \"Id\" ASC LIMIT 1";
    }

    public string GenerateSelectMultipleRecordsQuery<TEntity>(string propertyName, string? value)
    {
        var tableName = typeof(TEntity).GetReadTableName();

        //for checks where the criteria is a property being null
        if (value == null || value.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" IS NULL ORDER BY \"Id\" ASC";
        }
        return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" = '{value.Trim()}' ORDER BY \"Id\" ASC";
    }

    public string GenerateSelectLimitedRecordsQuery<TEntity>(string propertyName, string? value, int limit)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        //for checks where the criteria is a property being null
        if (value == null || value.Trim().Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" IS NULL ORDER BY \"Id\" ASC LIMIT {limit}";
        }
        return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" = '{value.Trim()}' ORDER BY \"Id\" ASC LIMIT {limit}";
    }

    public string GenerateSelectByMultipleValuesListQuery<TEntity>(string propertyName, IEnumerable<string> values)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        var formatValues = string.Join(", ", values.Select(v => $"'{v.Trim()}'"));
        return $"SELECT * FROM \"{tableName}\" WHERE \"{propertyName}\" IN ({formatValues})";
    }

    public string GenerateSelectByMultipleFieldsQuery<TEntity>(Dictionary<string, object> criteria, int? queryLimit)
    {
        var tableName = typeof(TEntity).GetReadTableName();
        var queryBuilder = new StringBuilder($"SELECT * FROM \"{tableName}\" WHERE ");

        var values = new List<string>();

        foreach (var item in criteria)
        {
            if (item.Value == null || item.Value.ToString()?.Trim().Equals("null", StringComparison.OrdinalIgnoreCase) == true)
            {
                // PostgreSQL requires "ColumnName" IS NULL
                values.Add($"\"{item.Key}\" IS NULL");
                continue;
            }
            var type = item.Value.GetType();        
            
            if (type.IsPrimitive || item.Value is decimal || type.IsEnum)
            {
                values.Add($"\"{item.Key}\" = {item.Value}");
            }
            else
            {
                values.Add($"\"{item.Key}\" = '{item.Value.ToString()?.Trim()}'");
            }
        }

        queryBuilder.Append(string.Join(" AND ", values));

        if (queryLimit != null)
        {
            queryBuilder.Append($" ORDER BY \"Id\" ASC LIMIT {queryLimit};");
        }
        else
        {
            queryBuilder.Append(" ORDER BY \"Id\" ASC;");
        }
        
        return queryBuilder.ToString();
    }
}