namespace BusBooking.Models.Entities
{
    public class Bus
    {
        public int Id {get; set; }
        public BusCapacity SeatCapacity { get; set; }
        public int RouteId { get; set; }
        public DateTime CreatedAt { get; set; }

        public Route Route {get; set; }

        public virtual Driver Driver { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}