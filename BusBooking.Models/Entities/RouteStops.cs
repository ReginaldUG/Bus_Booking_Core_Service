using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities;

[Table("RouteStops")]
[ReadTableName("RouteStops")]
[WriteTableName("RouteStops")]
public class RouteStops
{
    public int Id { get; set; }
    public int BusStopId {get; set; }
    public int RouteId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    public virtual Route Route {get; set; }
    public virtual BusStops BusStops { get; set; }
}