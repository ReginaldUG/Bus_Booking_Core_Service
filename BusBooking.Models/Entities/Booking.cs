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
        public int ScheduleId { get; set; }
        public decimal Price {get; set; }
        public bool Completed { get; set; } = false;
        public bool IsCancelled { get; set; } = false;
        public string? CancelledBy { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Customer Customer { get; set; } = null!;
        public virtual Schedule Schedule { get; set; } = null!;
    }
}