namespace BusBooking.Models.Entities;

public class BusStops
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<RouteStops> RouteStops { get; set; } = new List<RouteStops>();
    public virtual ICollection<Booking> Booking { get; set; } = new List<Booking>();
}