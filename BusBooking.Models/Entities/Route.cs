using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Routes")]
    [ReadTableName("Routes")]
    [WriteTableName("Routes")]
    public class Route
    {
        public int Id { get; set; }
        public required string RouteName { get; set; }
        public decimal Price { get; set; }
        public RouteType Type { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime {get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public virtual Bus? Bus {get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    }
}