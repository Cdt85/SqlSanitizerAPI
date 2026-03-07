using System.ComponentModel.DataAnnotations;

namespace SqlSanitizerAPI.Models.Requests
{
    /// <summary> Request model for user login. </summary>
    public class LoginRequest
    {
        /// <summary> Username for authentication. </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary> Password for authentication. </summary>
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}