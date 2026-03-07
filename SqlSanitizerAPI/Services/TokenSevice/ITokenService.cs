using SqlSanitizerAPI.Models;
using SqlSanitizerAPI.Models.Responses;

namespace SqlSanitizerAPI.Services.TokenSevice
{
    /// <summary>   Interface for JWT token generation and validation services. </summary>
    public interface ITokenService
    {
        /// <summary> Generates a JWT token for the specified username. </summary>
        /// <param name="username">The username to generate the token for.</param>
        /// <returns>A TokenResponse containing the token and metadata.</returns>
        TokenResponse GenerateToken(string username);

        /// <summary> Validates a JWT token. </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        bool ValidateToken(string token);
    }
}