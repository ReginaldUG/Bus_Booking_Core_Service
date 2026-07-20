using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Customers")]
    [ReadTableName("Customers")]
    [WriteTableName("Customers")]
    public class Customer
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int Age {get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string HashedPassword { get; set; }
        public bool EmailValidated { get; set; } = false;
        public string Status { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //Nav Properties
        public virtual CustomerWallet? Wallet { get; set; }     //Customer can have one wallet, One-To-One
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();   //One-to-Many customer can have many bokings
        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>(); 
    }
}