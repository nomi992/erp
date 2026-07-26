namespace erp_backend.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Request successful", int statusCode = StatusCodes.Status200OK) =>
        new()
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data,
        };

    public static ApiResponse<T> Fail(string message, int statusCode = StatusCodes.Status400BadRequest, List<string>? errors = null) =>
        new()
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Errors = errors,
        };
}
