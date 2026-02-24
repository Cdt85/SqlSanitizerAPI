using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OneOf;
using SqlSanitizerAPI.Models;
using SqlSanitizerAPI.Models.Responses;
using SqlSanitizerAPI.Repositories;
using System.Text.RegularExpressions;

namespace SqlSanitizerAPI.Services.SanitizationService
{
    /// <summary> Provides configuration options for the sanitization service. </summary>
    /// <remarks>Use this class to specify settings that control the behavior of the sanitization service, such as cache expiration policies.</remarks>
    public class SanitizationServiceOptions
    {
        public int CacheExpirationInMinutes { get; set; }
    }

    /// <summary> Provides services for sanitizing SQL queries by masking sensitive words and for managing the list of sensitive words used in sanitization operations.  </summary>
    /// <remarks>The service enables applications to sanitize SQL queries by replacing sensitive words with asterisks,
    /// and provides methods to insert, update, delete, and retrieve sensitive words.
    /// Caching is used to optimize performance and ensure up-to-date sensitive word lists are available for query sanitization.</remarks>
    /// <param name="logger">The logger used to record informational and error messages related to sanitization and sensitive word management.</param>
    /// <param name="repository">The repository used to access and modify sensitive word data in the underlying data store.</param>
    /// <param name="memoryCache">The memory cache used to store sensitive words in memory for improved performance and reduced database access.</param>
    /// <param name="options">The options that configure the behavior of the sanitization service, such as cache expiration settings.</param>
    public class SanitizationService(ILogger<SanitizationService> logger,
                                     IRepository repository,
                                     IMemoryCache memoryCache,
                                     IOptions<SanitizationServiceOptions> options) : ISanitizationService
    {
        private readonly ILogger<SanitizationService> _logger = logger;
        private readonly IRepository _repository = repository;
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly SanitizationServiceOptions _options = options.Value;

        /// <inheritdoc />
        public async Task<OneOf<string, ErrorDetails>> SanitizeSqlQueryAsync(string sqlQuery)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sqlQuery))
                {
                    return new ErrorDetails(400, "SQL query cannot be empty.");
                }

                var sensitiveWordsResult = await GetSensitiveWordsAsync();
                if (sensitiveWordsResult.IsT1)
                {
                    return sensitiveWordsResult.AsT1;
                }

                var sensitiveWords = sensitiveWordsResult.AsT0;
                var sanitizedQuery = sqlQuery;

                // Replace each sensitive word in the query with asterisks, ignoring case
                foreach (var word in sensitiveWords)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        sanitizedQuery = Regex.Replace(sanitizedQuery, Regex.Escape(word), new string('*', word.Length), RegexOptions.IgnoreCase);
                    }
                }

                _logger.LogInformation("Successfully sanitized SQL query");
                return sanitizedQuery;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sanitizing SQL query");
                return new ErrorDetails(500, "An error occurred while sanitizing the SQL query.");
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<int, ErrorDetails>> InsertSensitiveWordAsync(string word)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    return new ErrorDetails(400, "Word cannot be empty.");
                }

                var rowsAffected = await _repository.InsertSensitiveWordAsync(word);

                if (rowsAffected > 0)
                {
                    InvalidateAllCaches();
                    _logger.LogInformation("Successfully inserted sensitive word");
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while inserting sensitive word");

                // Map SQL error codes to appropriate HTTP status codes
                return ex.Message.Contains("already exists")
                    ? new ErrorDetails(409, ex.Message)
                    : new ErrorDetails(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inserting sensitive word");
                return new ErrorDetails(500, "An error occurred while inserting the sensitive word.");
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<List<string>, ErrorDetails>> GetSensitiveWordsAsync()
        {
            try
            {
                string cacheKey = CacheKeys.AllSensitiveWords;

                if (_memoryCache.TryGetValue(cacheKey, out List<string>? cachedWords) && cachedWords != null)
                {
                    _logger.LogInformation("Returning sensitive words from cache for key: {CacheKey}", cacheKey);
                    return cachedWords;
                }

                var sensitiveWords = await _repository.GetActiveSensitiveWordsAsync();

                if (sensitiveWords == null || !sensitiveWords.Any())
                {
                    var emptyList = new List<string>();
                    _memoryCache.Set(cacheKey, emptyList, TimeSpan.FromMinutes(_options.CacheExpirationInMinutes));
                    return emptyList;
                }

                List<string> result;

                result = sensitiveWords.Select(w => w.Word).ToList();

                _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.CacheExpirationInMinutes));
                _logger.LogInformation("Cached sensitive words for key: {CacheKey}", cacheKey);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensitive words.");
                return new ErrorDetails(500, "An error occurred while retrieving sensitive words.");
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<List<SensitiveWordsDetailResponse>, ErrorDetails>> ListSensitiveWordsDetailAsync()
        {
            try
            {
                var sensitiveWords = await _repository.GetActiveSensitiveWordsAsync();

                if (sensitiveWords == null || !sensitiveWords.Any())
                {
                    return new List<SensitiveWordsDetailResponse>();
                }

                // Map GetSensitiveWordsResponse to SensitiveWordsDetailResponse
                var result = sensitiveWords.Select(w => new SensitiveWordsDetailResponse
                {
                    Id = w.Id,
                    Word = w.Word
                }).OrderBy(w => w.Id).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sensitive words.");
                return new ErrorDetails(500, "An error occurred while retrieving sensitive words.");
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<int, ErrorDetails>> UpdateSensitiveWordAsync(int id, string word)
        {
            try
            {
                if (id <= 0)
                {
                    return new ErrorDetails(400, "Id must be greater than 0.");
                }

                if (string.IsNullOrWhiteSpace(word))
                {
                    return new ErrorDetails(400, "Word cannot be empty.");
                }

                var rowsAffected = await _repository.UpdateSensitiveWordAsync(id, word);

                if (rowsAffected == 0)
                {
                    return new ErrorDetails(404, $"Sensitive word with ID {id} not found or is inactive.");
                }

                if (rowsAffected > 0)
                {
                    InvalidateAllCaches();
                    _logger.LogInformation("Successfully updated sensitive word with ID: {Id}", id);
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while updating sensitive word with ID: {Id}", id);

                // Map SQL error codes to appropriate HTTP status codes
                if (ex.Message.Contains("does not exist") || ex.Message.Contains("inactive"))
                {
                    return new ErrorDetails(404, ex.Message);
                }
                else if (ex.Message.Contains("already exists"))
                {
                    return new ErrorDetails(409, ex.Message);
                }

                return new ErrorDetails(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating sensitive word with ID: {Id}", id);
                return new ErrorDetails(500, "An error occurred while updating the sensitive word.");
            }
        }

        /// <inheritdoc />
        public async Task<OneOf<int, ErrorDetails>> DeleteSensitiveWordAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ErrorDetails(400, "Id must be greater than 0.");
                }

                var rowsAffected = await _repository.DeleteSensitiveWordAsync(id);

                if (rowsAffected == 0)
                {
                    return new ErrorDetails(404, $"Sensitive word with ID {id} not found or is already inactive.");
                }

                if (rowsAffected > 0)
                {
                    InvalidateAllCaches();
                    _logger.LogInformation("Successfully deleted sensitive word with ID: {Id}", id);
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error while deleting sensitive word with ID: {Id}", id);

                // Map SQL error codes to appropriate HTTP status codes
                if (ex.Message.Contains("does not exist") || ex.Message.Contains("inactive"))
                {
                    return new ErrorDetails(404, ex.Message);
                }

                return new ErrorDetails(400, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sensitive word with ID: {Id}", id);
                return new ErrorDetails(500, "An error occurred while deleting the sensitive word.");
            }
        }

        /// <summary> Invalidates all cached sensitive words to ensure data consistency after modifications. </summary>
        private void InvalidateAllCaches()
        {
            // Remove the main cache entry for all words using strongly-typed key
            _memoryCache.Remove(CacheKeys.AllSensitiveWords);

            // Note: Individual word caches will expire naturally or can be tracked if needed
            _logger.LogInformation("Invalidated all sensitive words caches");
        }
    }
}