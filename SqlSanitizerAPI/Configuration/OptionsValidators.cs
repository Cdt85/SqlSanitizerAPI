using Microsoft.Extensions.Options;
using SqlSanitizerAPI.Repositories;
using SqlSanitizerAPI.Services.SanitizationService;

namespace SqlSanitizerAPI.Configuration
{
    /// <summary> Validator for RepositoryOptions using IValidateOptions pattern. </summary>
    public class RepositoryOptionsValidator : IValidateOptions<RepositoryOptions>
    {
        /// <summary> Validates the specified repository options and returns the result of the validation. </summary>
        /// <remarks>This method checks for the presence of a valid database connection string and schema, as well as ensuring that the SQL command timeout is greater than zero.
        /// It is essential to provide valid options to avoid validation failures.</remarks>
        /// <param name="name">The name of the repository being validated. This parameter can be null.</param>
        /// <param name="options">The options containing the database connection string, schema, and SQL command timeout settings that need to be validated.</param>
        /// <returns>A result indicating whether the validation succeeded or failed, along with any associated error messages if validation fails.</returns>
        public ValidateOptionsResult Validate(string? name, RepositoryOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return ValidateOptionsResult.Fail("Database connection string is required.");
            }

            if (string.IsNullOrWhiteSpace(options.DbSchema))
            {
                return ValidateOptionsResult.Fail("Database schema is required.");
            }

            if (options.SqlCommandDefaultTimeout <= 0)
            {
                return ValidateOptionsResult.Fail("SQL command timeout must be greater than 0.");
            }

            return ValidateOptionsResult.Success;
        }
    }

    /// <summary> Validator for SanitizationServiceOptions using IValidateOptions pattern. </summary>
    public class SanitizationServiceOptionsValidator : IValidateOptions<SanitizationServiceOptions>
    {
        /// <summary> Validates the specified sanitization options and ensures that the cache expiration time is within acceptable limits. </summary>
        /// <remarks>This method checks that the cache expiration time is greater than 0 minutes and does not exceed 1440 minutes (24 hours).</remarks>
        /// <param name="name">The name associated with the validation process, which may influence the context of the validation.</param>
        /// <param name="options">The options to validate, which include settings for cache expiration and other sanitization parameters.</param>
        /// <returns>A result indicating the success or failure of the validation, including any relevant error messages if validation fails.</returns>
        public ValidateOptionsResult Validate(string? name, SanitizationServiceOptions options)
        {
            if (options.CacheExpirationInMinutes <= 0)
            {
                return ValidateOptionsResult.Fail("Cache expiration must be greater than 0 minutes.");
            }

            if (options.CacheExpirationInMinutes > 1440) // 24 hours
            {
                return ValidateOptionsResult.Fail("Cache expiration should not exceed 1440 minutes (24 hours).");
            }

            return ValidateOptionsResult.Success;
        }
    }
}