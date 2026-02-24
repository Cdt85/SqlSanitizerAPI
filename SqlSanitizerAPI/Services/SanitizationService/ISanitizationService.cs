using OneOf;
using SqlSanitizerAPI.Models;
using SqlSanitizerAPI.Models.Responses;

namespace SqlSanitizerAPI.Services.SanitizationService
{
    /// <summary> Interface for the Sanitization Service, defining methods for SQL query sanitization and sensitive word management. </summary>
    public interface ISanitizationService
    {
        /// <summary> Asynchronously sanitizes a SQL query to prevent SQL injection attacks. </summary>
        /// <param name="sqlQuery">The SQL query to sanitize.</param>
        /// <returns>A sanitized SQL query or error details if the operation fails.</returns>
        Task<OneOf<string, ErrorDetails>> SanitizeSqlQueryAsync(string sqlQuery);

        /// <summary> Asynchronously inserts a new sensitive word into the data store. </summary>
        /// <param name="word">The sensitive word to insert.</param>
        /// <returns>The number of rows affected or error details if the operation fails.</returns>
        Task<OneOf<int, ErrorDetails>> InsertSensitiveWordAsync(string word);

        /// <summary>  Asynchronously retrieves a list of sensitive words associated with the specified identifier. </summary>
        /// <remarks>This method may return an empty list if no sensitive words are found for the given identifier.
        /// Ensure that the identifier is valid to avoid unexpected errors.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains either a list of sensitive words or an error details object if the operation fails.</returns>
        Task<OneOf<List<string>, ErrorDetails>> GetSensitiveWordsAsync();

        /// <summary> Asynchronously retrieves a collection of sensitive words with their associated details. </summary>
        /// <remarks>Use this method when you need to manage or review sensitive words within the system.
        /// The caller should handle both successful and error outcomes appropriately, as the result may indicate either a successful retrieval or an error condition.</remarks>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains either a list of sensitive word detail responses or an error details object if the operation fails.</returns>
        Task<OneOf<List<SensitiveWordsDetailResponse>, ErrorDetails>> ListSensitiveWordsDetailAsync();

        /// <summary> Updates an existing sensitive word with a new value asynchronously. </summary>
        /// <remarks>If the specified identifier does not correspond to an existing sensitive word, the operation will not update any records.
        /// Ensure that the input parameters meet the required constraints to avoid errors.</remarks>
        /// <param name="id">The unique identifier of the sensitive word to update. Must be a positive integer corresponding to an existing entry.</param>
        /// <param name="word">The new sensitive word to assign. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains either the number of affected rows if the update succeeds,
        /// or an error details object if the operation fails.</returns>
        Task<OneOf<int, ErrorDetails>> UpdateSensitiveWordAsync(int id, string word);

        /// <summary> Asynchronously deletes a sensitive word identified by its unique identifier. </summary>
        /// <remarks>This method removes a sensitive word from the underlying data store.
        /// Ensure that the specified identifier corresponds to an existing sensitive word. The operation may affect the state of the sensitive word list.</remarks>
        /// <param name="id">The unique identifier of the sensitive word to delete. Must be a positive integer.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// The task result contains the number of records deleted if the operation succeeds, or an error details object if the operation fails.</returns>
        Task<OneOf<int, ErrorDetails>> DeleteSensitiveWordAsync(int id);
    }
}