using System.Security.Claims;
using BusBooking.Models.DTO;
using Microsoft.AspNetCore.Http;

namespace BusBooking.Services.BL.Interfaces;

public interface IJwtService
{
    string GenerateJwtToken(JwtUserTokenRequest request);
    int? GetCurrentUserId(HttpContext context);
    bool isTokenExpired(string token);
    DateTime? GetExpiryDate(string token);
    string? GetEmail(string token);
    int? GetUserId(string token);
    ClaimsPrincipal? ValidateToken(string token);
}