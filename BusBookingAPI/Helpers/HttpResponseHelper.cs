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

        var type = response.GetType();
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
        {
            dynamic dynamicResponse = response;
            stringStatusCode = dynamicResponse.StatusCode ?? "200";
        }
        else if (response is ApiResponse baseResponse)
        {
            stringStatusCode = baseResponse.Status ? "200" : "400";
        }

        if (int.TryParse(stringStatusCode, out int numericStatusCode))
        {
            return new ObjectResult(response) { StatusCode = numericStatusCode };
        }

        return new ObjectResult(response) { StatusCode = 500 };
    }
}
