using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BusBooking.Models.DTO;
using BusBooking.Services.BL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BusBooking.Services.BL.Implementations;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(JwtUserTokenRequest request)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, request.UserEmail),
            new Claim("id", request.UserId.ToString())
        }.Union(request.Claims);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = _configuration.GetValue("JwtSettings:ExpiryInMinutes", 120);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? GetCurrentUserId(HttpContext context)
    {
        var userId = context.User.Claims.FirstOrDefault(x=>x.Type=="id")?.Value;
        return string.IsNullOrWhiteSpace(userId) ? null : int.Parse(userId);
    }

    public bool isTokenExpired(string token)
    {
        var expiry = GetExpiryDate(token);
        if (expiry == null)
            return true;

        return expiry <= DateTime.UtcNow;
    }

    public DateTime? GetExpiryDate(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return null;
        }
    }

    public string? GetEmail (string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        return principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;
    }

    public int? GetUserId (string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
            return null;

        var claim = principal.Claims.FirstOrDefault(x => x.Type == "id");
        return claim == null ? null : int.Parse(claim.Value);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Secret"]!))
            };

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

}