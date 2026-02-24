using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SqlSanitizerAPI.Models.Requests;
using SqlSanitizerAPI.Services.TokenSevice;

namespace SqlSanitizerAPI.Controllers
{
    /// <summary> Represents configuration options for user authentication, including the username and password credentials. </summary>
    /// <remarks>This class is intended for simplified user authentication scenarios.
    /// For production environments, consider using OAuth or other secure authentication mechanisms to enhance security.</remarks>
    public class AuthControllerOptions
    {
        // This is simplified user authentication - OAuth would be better for a real word scenario
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }

    /// <summary> Provides API endpoints for user authentication and JWT token generation. </summary>
    /// <remarks>This controller enables clients to authenticate by submitting credentials and, upon successful authentication, receive a JWT token for use in subsequent API requests.
    /// It is intended to be used in conjunction with a token service that manages token creation and validation.</remarks>
    /// <param name="tokenService">The service used to generate JWT tokens for authenticated users.</param>
    /// <param name="options">The configuration options containing authentication credentials for the controller.</param>
    [ApiVersion(1)]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController(ITokenService tokenService, IOptions<AuthControllerOptions> options) : ControllerBase
    {
        private readonly ITokenService _tokenService = tokenService;
        private readonly string _username = options.Value.Username;
        private readonly string _password = options.Value.Password;

        /// <summary> Authenticates a user and returns a JWT token. </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token if authentication is successful</returns>
        [HttpPost("login")]
        [ProducesResponseType<object>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Username != _username || request.Password != _password)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            var tokenResponse = _tokenService.GenerateToken(request.Username);

            return Ok(tokenResponse);
        }
    }
}