namespace BusBooking.Models.DTO
{
    public class ApiResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; }

        public static ApiResponse Success(string message)
        {
            return new ApiResponse
            {
                Status = true,
                Message = message
            };
        }

        public static ApiResponse Failure(string message)
        {
            return new ApiResponse
            {
                Status = false,
                Message = message
            };
        }

    }

    
    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
        public string StatusCode { get; set; }

        public static ApiResponse<T> Success(string message, string statusCode, T data)
        {
            return new ApiResponse<T>
            {
                Status = true,
                Message = message,
                StatusCode = statusCode,
                Data = data
            };
        }

        public static ApiResponse<T> Failure(string message, string statusCode)
        {
            return new ApiResponse<T>
            {
                Status = false,
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}