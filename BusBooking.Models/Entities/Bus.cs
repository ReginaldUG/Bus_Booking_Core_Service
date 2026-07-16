using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Buses")]
    [ReadTableName("Buses")]
    [WriteTableName("Buses")]
    public class Bus
    {
        public int Id {get; set; }
        public BusCapacity SeatCapacity { get; set; }
        public string PlateNumber {get; set; }
        public bool DriverAssigned { get; set; } = false;
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Driver Driver { get; set; }
    }
}