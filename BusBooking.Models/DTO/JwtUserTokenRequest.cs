using System.Security.Claims;

namespace BusBooking.Models.DTO;

public class JwtUserTokenRequest
{
    public int UserId { get; set; }
    public required string UserEmail { get; set; }
    public IEnumerable<Claim> Claims { get; set; } = new List<Claim>();
}