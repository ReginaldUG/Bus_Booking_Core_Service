using System.Security.Claims;
using BusBooking.Services.BL.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BusBooking.Services.BL.Implementations;

public class AuthenticatedUserService : IAuthenticatedUserService
{
    public AuthenticatedUserService (IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user != null)
        {
            var userIdClaim = user.FindFirst("id")?.Value;
            UserId = string.IsNullOrWhiteSpace(userIdClaim) ? null : int.Parse(userIdClaim);
            Email = user.FindFirst(ClaimTypes.Email)?.Value;
        }
    }

    public int? UserId { get; }
    public string? Email { get; }
}