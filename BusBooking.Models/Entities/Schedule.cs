using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities;

[Table("Schedules")]
[ReadTableName("Schedules")]
[WriteTableName("Schedules")]
public class Schedule
{
    public int Id { get; set; }
    public int? BusId { get; set; }
    public int RouteId { get; set; }
    public DateOnly DateOfDeparture { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public TimeOnly ArrivalTime { get; set; }
    public string Status { get; set; }
    public int? AvailableSeats {get; set; }
    public int CreatedFromTemplateID { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Route Route { get; set; }
    public virtual Bus? Bus { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();


}