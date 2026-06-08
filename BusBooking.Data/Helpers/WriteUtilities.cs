using System.Text;
using BusBooking.Data.Extensions;

namespace BusBooking.Data.Helpers;

public class WriteUtilities
{
    public string GenerateInsertQuery<TEntity>() where TEntity : class
    {
        var tableName = typeof(TEntity).GetWriteTableName();

        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id" && !p.PropertyType.IsGenericType && !p.PropertyType.IsClass ||
                        p.PropertyType == typeof(string))
            .Select(p => p.Name)
            .ToList();

        var insertQuery = new StringBuilder($"INSERT INTO \"{tableName}\" (");

        insertQuery.Append(string.Join(", ", properties.Select(p => $"\"{p}\"")));
        insertQuery.Append(") VALUES (");
        insertQuery.Append(string.Join(", ", properties.Select(p => $"@{p}")));
        insertQuery.Append(");");

        return insertQuery.ToString();
    }

    public string GenerateUpdateQuery<TEntity>() where TEntity : class
    {
        var tableName = typeof(TEntity).GetWriteTableName();

        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "CustomerId" && (!p.PropertyType.IsGenericType && !p.PropertyType.IsClass || p.PropertyType == typeof(string)))
            .Select(p => p.Name)
            .ToList();
        
        var updateQuery = new StringBuilder($"UPDATE \"{tableName}\" SET ");

        var setClauses = properties.Select(p => $"\"{p}\" = @{p}");
        updateQuery.Append(string.Join(", ", setClauses));
        updateQuery.Append(" WHERE \"Id\" = @Id;");

        return updateQuery.ToString();
    }
}
