using System.Net;
using System.Text.Json;

namespace Application.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentException:
                case InvalidOperationException:
                    response.Message = exception.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case KeyNotFoundException:
                    response.Message = "The requested resource was not found.";
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case UnauthorizedAccessException:
                    response.Message = "You are not authorized to access this resource.";
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                case HttpRequestException:
                    response.Message = "External service error occurred.";
                    response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                    break;

                default:
                    response.Message = "An internal server error occurred.";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}