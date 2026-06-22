using BusBooking.Core.Constants;
using BusBooking.Models.DTO;

namespace BusBookingAPI.Helpers;

public class GeneralHelpers
{
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
    
}