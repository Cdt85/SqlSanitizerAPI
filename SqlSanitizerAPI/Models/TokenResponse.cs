namespace SqlSanitizerAPI.Models
{
    /// <summary>
    /// Response model for token generation.
    /// </summary>
    public class TokenResponse
    {
        /// <summary> The JWT token. </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary> Token expiration time in seconds. </summary>
        public int ExpiresIn { get; set; }

        /// <summary> The type of token (Bearer). </summary>
        public string TokenType { get; set; } = "Bearer";
    }
}
