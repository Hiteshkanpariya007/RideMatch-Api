using Common.Exceptions;
using System.Text.Json;
using FluentValidation;

namespace RideMatch_Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;

        private readonly ILogger<ExceptionHandlingMiddleware> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionHandlingMiddleware"/> class.
        /// </summary>
        /// <param name="next">next as RequestDelegate.</param>
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await WriteErrorAsync(context, exception);
            }
        }

        private async Task WriteErrorAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                ValidationException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                ConflictException => StatusCodes.Status409Conflict,
                BusinessRuleException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled request error. TraceId: {TraceId}", context.TraceIdentifier);
            }

            var errors = exception is ValidationException validationException
                ? validationException.Errors.Select(error => error.ErrorMessage).ToArray()
                : Array.Empty<string>();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                success = false,
                message = statusCode == 500 ? "Unable to process request." : exception.Message,
                errors,
                traceId = context.TraceIdentifier
            }));
        }
    }
}
