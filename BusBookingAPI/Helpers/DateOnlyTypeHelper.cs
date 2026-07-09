namespace BusBookingAPI.Helpers;

using System.Data;
using Dapper;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.DbType = DbType.Date;
        // Forces clean numerical database ISO formatting
        parameter.Value = value.ToString("yyyy-MM-dd"); 
    }

    public override DateTime Parse(object value)
    {
        // If the database drops back a full DateTime stamp, accept it
        if (value is DateTime dateTime)
            return dateTime;

        // If it comes back from a raw database driver layer as a DateOnly struct profile, handle cleanly
        if (value is DateOnly dateOnly)
            return dateOnly.ToDateTime(TimeOnly.MinValue);

        return Convert.ToDateTime(value);
    }
}
