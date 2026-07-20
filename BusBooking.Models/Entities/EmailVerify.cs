using System.ComponentModel.DataAnnotations.Schema;
using BusBooking.Core.Attributes;

namespace BusBooking.Models.Entities;

[Table("EmailVerify")]
[ReadTableName("EmailVerify")]
[WriteTableName("EmailVerify")]
public class EmailVerify
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string CodeHash { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}