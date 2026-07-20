using System.Security.Cryptography;
using BusBooking.Core.Constants;
using BusBooking.Core.Enums;
using BusBooking.Models.DTO;
using BusBooking.Models.DTO.ResponseDTOs;

namespace BusBooking.Services.Helpers;

public class GeneralHelpers
{
    public ApiResponse<RandStringResponseDTO> _rand_string (int num = 64, int s = 10)
    {
        string alphabet = string.Concat(
            Enumerable.Range('a', 26).Select(c => (char)c)
            .Concat(Enumerable.Range('A', 26).Select(c => (char)c))
            .Concat(Enumerable.Range('0', 10).Select(c => (char)c))
        );

        string rand = RandomNumberGenerator.GetString(alphabet, num);
        string salt = RandomNumberGenerator.GetString(alphabet, s);

        return ApiResponse<RandStringResponseDTO>.Success("Rand Generated", new RandStringResponseDTO
        {
            Rand = rand,
            Salt = salt
        });
    }

    public ApiResponse<int> TokenGenerator()
    {
        int token = RandomNumberGenerator.GetInt32(100000, 999999);
        return ApiResponse<int>.Success("Token generated", token);
    }

    public ApiResponse ValidatePlateNumberFormat (string plateNumber)
    {
        //Validate string length
        if (string.IsNullOrEmpty(plateNumber) || plateNumber.Length != 8)
            return ApiResponse.Failure(ErrorMessages.INVALID_PLATE_NUMBER);
        
        //extract plate number parts
        string prefix = plateNumber.Substring(0, 3);
        string intValues = plateNumber.Substring(3,3);
        string suffix = plateNumber.Substring(7);

        //Ensure all 3 are string, int, string
        if (!intValues.All(char.IsDigit))
            return ApiResponse.Failure(ErrorMessages.INVALID_PLATE_NUMBER);
        
        if (!prefix.All(char.IsLetter) || !suffix.All(char.IsLetter))
            return ApiResponse.Failure(ErrorMessages.INVALID_PLATE_NUMBER);
        
        return ApiResponse.Success("Valid plate number format");
    }

    public ApiResponse<int> MapToWorkingDaysEnum (string day)
    {
        if (Enum.TryParse(day, true, out WorkingDays matchedDay))
        {
            int IntValue = (int)matchedDay;
            return ApiResponse<int>.Success("matched", IntValue);
        }
        else
        {
            return ApiResponse<int>.Failure("failed to match", StatusCodes.BadRequest);
        }
    }
}