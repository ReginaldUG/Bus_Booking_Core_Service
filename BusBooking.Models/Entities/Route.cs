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
        public string Type { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public bool BusAssigned { get; set; } = false;
        public TimeOnly? ArrivalTime {get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public virtual Bus? Bus {get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}