using System.Text.RegularExpressions;
using BusBooking.Models.DTO;
using Isopoh.Cryptography.Argon2;

namespace BusBookingAPI.Helpers;

public class AuthenticationHelper
{
    public ApiResponse<string> HashPassword(string password)
    {
        var hashed = Argon2.Hash(password);
        return ApiResponse<string>.Success("Password hashed successfully", hashed);
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

    public ApiResponse ValidatePasswordRules(string password)
    {
        string? message =
            password.Length < 8 ? "Password must be at least 8 characters long" :
            !Regex.IsMatch(password, "[A-Z]") ? "Password must contain at least one uppercase" :
            !Regex.IsMatch(password, "[a-z]") ? "Password must contain at least one lower case" :
            !Regex.IsMatch(password, @"\d") ? "Password must contain at least one number" :
            !Regex.IsMatch(password, @"\W") ? "Password must contain at least one speacial character" :
            null;

        return message == null ? ApiResponse.Success("Password Verification Passed") : ApiResponse.Failure(message);
    }
}