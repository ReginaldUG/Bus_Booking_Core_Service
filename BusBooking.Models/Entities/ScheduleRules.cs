using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using BusBooking.Core.Attributes;
using BusBooking.Core.Enums;

namespace BusBooking.Models.Entities;

[Table("ScheduleRules")]
[ReadTableName("ScheduleRules")]
[WriteTableName("ScheduleRules")]
public class ScheduleRules
{
    public int Id { get; set; }
    public int RouteId { get; set; }
    public WorkingDays DayOfWeek { get; set; }
    public TimeOnly ScheduledDepartureTime { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Route Route { get; set; }
}