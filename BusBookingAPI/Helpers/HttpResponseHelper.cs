using BusBooking.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace BusBookingAPI.Helpers;

public static class HttpResponseHelper
{
    public static IActionResult GetHttpResponse(object response)
    {
        if (response == null)
        {
            return new ObjectResult(new { status = false, message = "No response payload." }) { StatusCode = 500 };
        }

        string stringStatusCode = "200"; // Fallback default

        // 1. Safely handle Generic ApiResponse<T> payloads (Registration, Login, etc.)
        // We use 'dynamic' here to easily read properties from any closed generic variant like ApiResponse<CustomerRegisterResponseDTO>
        var type = response.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            dynamic dynamicResponse = response;
            stringStatusCode = dynamicResponse.StatusCode ?? "200";
        }
        // 2. Handle base ApiResponse payloads (Actions with no data payload returned)
        else if (response is ApiResponse baseResponse)
        {
            stringStatusCode = baseResponse.Status ? "200" : "400";
        }

        // 3. Parse your HTTP string ("409", "200") into a true HTTP network status code
        if (int.TryParse(stringStatusCode, out int numericStatusCode))
        {
            return new ObjectResult(response) { StatusCode = numericStatusCode };
        }

        return new ObjectResult(response) { StatusCode = 500 };
    }
}
