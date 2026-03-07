namespace SqlSanitizerAPI.Models
{
    /// <summary> Represents error details returned from service operations. </summary>
    public class ErrorDetails
    {
        /// <summary> Gets or sets the HTTP status code associated with the error. </summary>
        public int ErrorCode { get; set; }

        /// <summary> Gets or sets the error message describing what went wrong. </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary> Initializes a new instance of the <see cref="ErrorDetails"/> class. </summary>
        public ErrorDetails()
        {
            // Intentionally left blank
        }

        /// <summary> Initializes a new instance of the <see cref="ErrorDetails"/> class with specified error code and message. </summary>
        /// <param name="errorCode">The HTTP status code</param>
        /// <param name="errorMessage">The error message</param>
        public ErrorDetails(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}