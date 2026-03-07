using System.ComponentModel.DataAnnotations;

namespace SqlSanitizerAPI.Models.Requests
{
    /// <summary> Request model for updating a sensitive word. </summary>
    public class UpdateSanitizeStringRequest
    {
        /// <summary> The ID of the sensitive word to update. </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id must be greater than 0")]
        public int Id { get; set; }

        /// <summary> The new value for the sensitive word. </summary>
        [Required(ErrorMessage = "SanitizeString is required")]
        [MinLength(1, ErrorMessage = "SanitizeString cannot be empty")]
        [MaxLength(255, ErrorMessage = "SanitizeString cannot exceed 255 characters")]
        public string SanitizeString { get; set; } = string.Empty;

        /// <summary> Parameterless constructor for model binding. </summary>
        public UpdateSanitizeStringRequest()
        {
            // Intentionally left blank
        }

        /// <summary> Initializes a new instance of the UpdateSanitizeStringRequest class with the specified identifier and sanitize string. </summary>
        /// <param name="id">The unique identifier for the request. Must be a positive integer.</param>
        /// <param name="sanitizeString">The string to be sanitized. Cannot be null or empty.</param>
        public UpdateSanitizeStringRequest(int id, string sanitizeString)
        {
            Id = id;
            SanitizeString = sanitizeString;
        }
    }
}