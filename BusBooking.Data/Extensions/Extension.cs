using BusBooking.Core.Attributes;

namespace BusBooking.Data.Extensions; 

public static class Extension
{
    public static string? GetReadTableName(this Type entity)
    {
        if (!Attribute.IsDefined(entity, typeof(ReadTableNameAttribute), false))
            return null;
        var tableNameAttribute = (ReadTableNameAttribute?)Attribute.GetCustomAttribute(entity, typeof(ReadTableNameAttribute));
        return tableNameAttribute?.Name;
    }
    public static string? GetWriteTableName(this Type entity)
    {
        if (!Attribute.IsDefined(entity, typeof(WriteTableNameAttribute), false))
            return null;

        var tableNameAttribute =
            (WriteTableNameAttribute?)Attribute.GetCustomAttribute(entity, typeof(WriteTableNameAttribute));

        return tableNameAttribute?.Name;
    }
}