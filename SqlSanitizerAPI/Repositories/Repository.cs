using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SqlSanitizerAPI.Models.Responses;
using System.Data;

namespace SqlSanitizerAPI.Repositories
{
    /// <summary> Represents configuration options for a repository, including connection details, database schema, command timeout, and logging preferences. </summary>
    /// <remarks>Use this class to specify settings required for establishing and managing database connections within a repository.
    /// Proper configuration of these options is essential for reliable and secure database operations.
    /// All properties should be set before using the repository to ensure correct behavior.</remarks>
    public class RepositoryOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DbSchema { get; set; } = string.Empty;
        public int SqlCommandDefaultTimeout { get; set; }
        public bool LogConnectionMessages { get; set; }
    }

    /// <summary> Provides methods for accessing and managing sensitive words in the database. </summary>
    /// <remarks>This class implements the IRepository interface and is responsible for database interactions related to sensitive words.
    /// It handles connection management and logging of operations.</remarks>
    /// <param name="options">The options used to configure the repository, including the connection string, database schema, and command timeout settings.</param>
    /// <param name="logger">The logger used for logging information and errors related to database operations.</param>
    public class Repository(IOptions<RepositoryOptions> options,
                            ILogger<Repository> logger) : IRepository
    {
        private readonly ILogger<Repository> _logger = logger;
        private readonly string _connectionString = options.Value.ConnectionString;
        private readonly string _dbSchema = options.Value.DbSchema;
        private readonly int _sqlCommandDefaultTimeout = options.Value.SqlCommandDefaultTimeout;
        private readonly bool _logConnectionMessages = options.Value.LogConnectionMessages;

        /// <inheritdoc />
        public async Task<int> InsertSensitiveWordAsync(string word, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = GetOpenSqlConnection();
                using var command = new SqlCommand($"{_dbSchema}.insert_AddSensitiveWord", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = _sqlCommandDefaultTimeout
                };

                command.Parameters.Add(new SqlParameter("@Word", SqlDbType.NVarChar, 100) { Value = word ?? (object)DBNull.Value });

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Executing stored procedure: {StoredProcedure} with Word: {Word}", command.CommandText, word);
                }

                // Execute and read the RowsAffected result
                using var reader = await command.ExecuteReaderAsync(cancellationToken);

                int rowsAffected = 0;
                if (await reader.ReadAsync(cancellationToken))
                {
                    rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
                }

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Inserted/reactivated sensitive word: {Word}. Rows affected: {RowsAffected}", word, rowsAffected);
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while inserting sensitive word: {Word}", word);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while inserting sensitive word: {Word}", word);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<GetSensitiveWordsResponse>> GetActiveSensitiveWordsAsync(CancellationToken cancellationToken = default)
        {
            var results = new List<GetSensitiveWordsResponse>();

            try
            {
                using var connection = GetOpenSqlConnection();
                using var command = new SqlCommand($"{_dbSchema}.select_GetActiveSensitiveWords", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = _sqlCommandDefaultTimeout
                };

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Executing stored procedure: {StoredProcedure}", command.CommandText);
                }

                using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    results.Add(new GetSensitiveWordsResponse(
                        id: reader.GetInt32(reader.GetOrdinal("Id")),
                        word: reader.GetString(reader.GetOrdinal("Word")),
                        createdAt: reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    ));
                }

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Retrieved {Count} active sensitive words", results.Count);
                }

                return results;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while retrieving active sensitive words");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while retrieving active sensitive words");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> UpdateSensitiveWordAsync(int id, string word, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = GetOpenSqlConnection();
                using var command = new SqlCommand($"{_dbSchema}.update_sensitive_word", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = _sqlCommandDefaultTimeout
                };

                command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
                command.Parameters.Add(new SqlParameter("@Word", SqlDbType.NVarChar, 100) { Value = word ?? (object)DBNull.Value });

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Executing stored procedure: {StoredProcedure} with Id: {Id}, Word: {Word}", command.CommandText, id, word);
                }

                // Execute and read the RowsAffected result
                using var reader = await command.ExecuteReaderAsync(cancellationToken);

                int rowsAffected = 0;
                if (await reader.ReadAsync(cancellationToken))
                {
                    rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
                }

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Updated sensitive word with Id: {Id}. Rows affected: {RowsAffected}", id, rowsAffected);
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while updating sensitive word with Id: {Id}, Word: {Word}", id, word);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating sensitive word with Id: {Id}, Word: {Word}", id, word);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteSensitiveWordAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = GetOpenSqlConnection();
                using var command = new SqlCommand($"{_dbSchema}.delete_sensitive_word", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = _sqlCommandDefaultTimeout
                };

                command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Executing stored procedure: {StoredProcedure} with Id: {Id}", command.CommandText, id);
                }

                // Execute and read the RowsAffected result
                using var reader = await command.ExecuteReaderAsync(cancellationToken);

                int rowsAffected = 0;
                if (await reader.ReadAsync(cancellationToken))
                {
                    rowsAffected = reader.GetInt32(reader.GetOrdinal("RowsAffected"));
                }

                if (_logConnectionMessages)
                {
                    _logger.LogInformation("Deleted sensitive word with Id: {Id}. Rows affected: {RowsAffected}", id, rowsAffected);
                }

                return rowsAffected;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred while deleting sensitive word with Id: {Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting sensitive word with Id: {Id}", id);
                throw;
            }
        }

        #region Private Methods

        /// <summary> Creates and opens a new SQL connection using the configured connection string. </summary>
        /// <returns>An open <see cref="SqlConnection"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the connection string is not configured.</exception>
        private SqlConnection GetOpenSqlConnection()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured.");
            }

            var connection = new SqlConnection(_connectionString);

            if (_logConnectionMessages)
            {
                _logger.LogInformation("Opening SQL connection to: {Server}", connection.DataSource);
            }

            connection.Open();
            return connection;
        }

        #endregion Private Methods
    }
}