namespace SolutionPruebaTecnica.Presentation.Exceptions;

public class ErrorResponse
{
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public int StatusCode { get; set; } = default!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ErrorResponse FromException(Exception ex, int statusCode)
    {
        return new ErrorResponse
        {
            Type = ex.GetType().Name,
            Title = GetTitleFromStatus(statusCode),
            Message = ex.Message,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    private static string GetTitleFromStatus(int statusCode)
    {
        return statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            500 => "Internal Server Error",
            _ => "Error"
        };
    }

}


