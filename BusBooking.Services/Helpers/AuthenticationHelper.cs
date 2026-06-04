using BusBooking.Models.DTO;
using Isopoh.Cryptography.Argon2;

namespace BusBookingAPI.Helpers;

public class AuthenticationHelper
{
    public ApiResponse<string> HashPassword(string password)
    {
        var hashed = Argon2.Hash(password);
        return ApiResponse<string>.Success("Password hashed successfully", "200", hashed);
    }

    public ApiResponse VerifyPassword(string password, string hashedPassword)
    {
        bool isValid = Argon2.Verify(hashedPassword, password);
        if (isValid)
        {
            return ApiResponse.Success("Password is valid");
        }
        else
        {
            return ApiResponse.Failure("Invalid Password");
        }
    }
}