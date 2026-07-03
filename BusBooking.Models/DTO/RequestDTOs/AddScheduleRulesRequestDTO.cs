using System.ComponentModel.DataAnnotations;
using BusBooking.Core.Enums;

namespace BusBooking.Models.DTO.RequestDTOs;

public class AddScheduleRulesRequestDTO
{
    [Required]
    public int RouteId { get; set; }

    [Required, EnumDataType(typeof(WorkingDays), ErrorMessage = "Day of week must be between 1 (Sunday) and 7 (Saturday)")]
    public WorkingDays DayOfWeek { get; set; }

    [Required]
    public TimeOnly ScheduledDepartureTime {get; set; }

    [Required]
    public TimeSpan EstimatedDuration { get; set; }
}