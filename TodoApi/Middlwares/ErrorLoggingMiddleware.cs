using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // קוד לפני הבקשה
        await _next(context); // המשך לעבד את הבקשה

        // קוד לאחר הבקשה
        if (context.Response.StatusCode >= 400) // אם יש שגיאה
        {
            var message = $"Error occurred: {context.Response.StatusCode} - {context.Response.ContentType}";
            _logger.LogError(message);
        }
    }
}
