using System.Data;
using Dapper;

namespace BusBookingAPI.Helpers;

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
    public override void SetValue(IDbDataParameter parameter, TimeOnly value)
    {
        parameter.Value = value;
    }
    public override TimeOnly Parse(object value)
    {
        if (value is TimeOnly timeOnly)
        {
            return timeOnly;
        }

        if (value is TimeSpan timeSpan)
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }

        if (value is DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        if (value is string stringTime && TimeOnly.TryParse(stringTime, out var parsedStringTime))
        {
            return parsedStringTime;
        }

        throw new InvalidCastException($"Cannot convert data type {value.GetType().FullName} to System.TimeOnly");
    }
    
}