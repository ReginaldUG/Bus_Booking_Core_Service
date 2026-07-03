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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<ScheduleRules> ScheduleRules { get; set; } = new List<ScheduleRules>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}