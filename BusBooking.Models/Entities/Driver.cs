using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Drivers")]
    [ReadTableName("Drivers")]
    [WriteTableName("Drivers")]
    public class Driver
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int Age {get; set; }
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public AccountStatus Status { get; set; }
        public int BusId { get; set; }
        public virtual Bus Bus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}