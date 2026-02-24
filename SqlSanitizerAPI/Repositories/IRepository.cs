using SqlSanitizerAPI.Models.Responses;

namespace SqlSanitizerAPI.Repositories
{
    /// <summary> Defines a contract for managing sensitive words in a data store, 
    /// including operations to insert, retrieve, update, and delete sensitive word entries asynchronously. </summary>
    /// <remarks>All methods in this interface are asynchronous and accept a cancellation token to support cooperative cancellation. 
    /// Implementations should ensure thread safety and data integrity when accessed concurrently.</remarks>
    public interface IRepository
    {
        /// <summary> Asynchronously inserts a new sensitive word or reactivates an existing inactive word. </summary>
        /// <remarks>This method executes a stored procedure that:
        /// - Validates the word parameter is not null or empty
        /// - Trims whitespace from the word
        /// - Checks if the word already exists and is active (raises error if true)
        /// - Reactivates the word if it exists but is inactive
        /// - Inserts a new word if it doesn't exist
        /// The operation is wrapped in a transaction for data consistency.</remarks>
        /// <param name="word">The sensitive word to add or reactivate. Must not be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>The number of rows affected by the operation (should be 1 if successful).</returns>
        /// <exception cref="SqlException">Thrown when the stored procedure encounters an error (e.g., null/empty word, duplicate active word).</exception>
        Task<int> InsertSensitiveWordAsync(string word, CancellationToken cancellationToken = default);

        /// <summary> Asynchronously retrieves a read-only list of active sensitive words from the database. </summary>
        /// <remarks>This method executes a stored procedure to obtain the current set of active sensitive words.
        /// Exceptions related to database connectivity or execution may be thrown if errors occur during retrieval.</remarks>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A read-only list of <see cref="GetSensitiveWordsResponse"/> objects representing the active sensitive words.
        /// The list is empty if no active sensitive words are found.</returns>
        Task<IReadOnlyList<GetSensitiveWordsResponse>> GetActiveSensitiveWordsAsync(CancellationToken cancellationToken = default);

        /// <summary> Asynchronously updates an existing sensitive word in the database. </summary>
        /// <remarks>This method executes a stored procedure that:
        /// - Validates that both Id and Word parameters are provided and not empty
        /// - Trims whitespace from the word
        /// - Checks if the record exists and is active (raises error if not found or inactive)
        /// - Checks if the new word already exists for a different Id (raises error if duplicate)
        /// - Updates the word for the specified Id
        /// The stored procedure ensures data integrity by preventing duplicate words.</remarks>
        /// <param name="id">The unique identifier of the sensitive word to update. Must be greater than 0.</param>
        /// <param name="word">The new value for the sensitive word. Must not be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>The number of rows affected by the update operation (should be 1 if successful).</returns>
        /// <exception cref="SqlException">Thrown when the stored procedure encounters an error (e.g., invalid parameters, record not found, duplicate word).</exception>
        Task<int> UpdateSensitiveWordAsync(int id, string word, CancellationToken cancellationToken = default);

        /// <summary> Asynchronously deletes (soft delete) a sensitive word by setting its IsActive flag to false. </summary>
        /// <remarks>This method executes a stored procedure to mark a sensitive word as inactive.
        /// The stored procedure validates that the ID is provided and that the record exists and is currently active.
        /// If validation fails, a SqlException will be thrown with the appropriate error message.</remarks>
        /// <param name="id">The unique identifier of the sensitive word to delete.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>The number of rows affected by the delete operation (should be 1 if successful, 0 if no matching record found).</returns>
        /// <exception cref="SqlException">Thrown when the stored procedure encounters an error (e.g., invalid ID, record not found, or already inactive).</exception>
        Task<int> DeleteSensitiveWordAsync(int id, CancellationToken cancellationToken = default);
    }
}