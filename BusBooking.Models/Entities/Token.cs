using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities
{
    [Table("Tokens")]
    [ReadTableName("Tokens")]
    [WriteTableName("Tokens")]
    public class Token
    {
        public int Id {get; set; }
        public int CustomerId { get; set; }
        public string TokenHash { get; set; }
        public string Salt { get; set; }
        public bool Revoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
}


