using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SqlSanitizerAPI.Middleware
{
    /// <summary> Provides a global exception handler for ASP.NET Core applications that logs unhandled exceptions and returns a
    /// standardized error response to the client. </summary>
    /// <remarks>This class implements the IExceptionHandler interface to ensure consistent error handling across the application.
    /// When an unhandled exception occurs, it logs the error and returns a ProblemDetails response with a 500 Internal Server Error status code.
    /// In development environments, the exception message is included in the response; otherwise, a generic error message is provided.
    /// Use this handler to centralize exception management and improve client-facing error responses.</remarks>
    /// <param name="logger">The logger used to record details about unhandled exceptions encountered during request processing.</param>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = logger;

        /// <summary> Attempts to handle an unhandled exception that occurs during the processing of an HTTP request by logging
        /// the error and returning a standardized JSON response. </summary>
        /// <remarks>The method logs the exception and writes a JSON response containing problem details to the HTTP response.
        /// In development environments, the response includes the exception message; otherwise, a generic support message is provided.</remarks>
        /// <param name="httpContext">The HTTP context for the current request, providing access to request and response information.</param>
        /// <param name="exception">The exception that was thrown during request processing and needs to be handled.</param>
        /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A value indicating whether the exception was handled successfully.
        /// Returns <see langword="true"/> if the exception was handled; otherwise, <see langword="false"/>.</returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An error occurred while processing your request",
                Detail = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                    ? exception.Message
                    : "Please contact support if the problem persists"
            };

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}