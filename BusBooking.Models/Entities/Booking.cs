using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Bookings")]
    [ReadTableName("Bookings")]
    [WriteTableName("Bookings")]
    public class Booking
    {
        public int Id { get; set; }
        public int CustomerId {get; set; }
        public int BusId {get; set; }
        public int RouteId{get; set; }
        public decimal Price {get; set; }
        public bool Completed { get; set; }
        public bool isPaid { get; set; }

        public virtual Customer Customer { get; set; } = null!;
        public virtual Bus Bus { get; set; } = null!;
        public virtual Route Route { get; set; } = null!;
    }
}