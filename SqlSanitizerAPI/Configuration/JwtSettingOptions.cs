using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace SqlSanitizerAPI.Configuration
{
    /// <summary> Configuration settings for JWT authentication. </summary>
    public class JwtSettingOptions
    {
        public const string SectionName = "Jwt";

        /// <summary> The secret key used for signing JWT tokens. </summary>
        [Required(ErrorMessage = "JWT Key is required")]
        [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters for security")]
        public string Key { get; set; } = string.Empty;

        /// <summary> The issuer of the JWT token. </summary>
        [Required(ErrorMessage = "JWT Issuer is required")]
        public string Issuer { get; set; } = string.Empty;

        /// <summary> The audience for the JWT token. </summary>
        [Required(ErrorMessage = "JWT Audience is required")]
        public string Audience { get; set; } = string.Empty;

        /// <summary> Token expiration time in minutes. </summary>
        [Range(1, 1440, ErrorMessage = "Token expiration must be between 1 and 1440 minutes (24 hours)")]
        public int ExpirationMinutes { get; set; } = 60;
    }

    /// <summary> Validator for JwtSettings using IValidateOptions pattern. </summary>
    public class JwtSettingsValidator : IValidateOptions<JwtSettingOptions>
    {
        /// <summary> Validates the JwtSettingOptions instance. </summary>
        /// <param name="name">The name of the options instance being validated.</param>
        /// <param name="options">The JwtSettingOptions instance to validate.</param>
        /// <returns>A ValidateOptionsResult indicating the validation outcome.</returns>
        public ValidateOptionsResult Validate(string? name, JwtSettingOptions options)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(options);

            if (!Validator.TryValidateObject(options, context, validationResults, validateAllProperties: true))
            {
                var errors = validationResults.Select(r => r.ErrorMessage ?? "Validation error");
                return ValidateOptionsResult.Fail(errors);
            }

            // Additional custom validation
            if (options.Key.Length < 32)
            {
                return ValidateOptionsResult.Fail("JWT Key is too short for secure token generation.");
            }

            return ValidateOptionsResult.Success;
        }
    }
}