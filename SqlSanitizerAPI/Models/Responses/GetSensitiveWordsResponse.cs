namespace SqlSanitizerAPI.Models.Responses
{
    /// <summary> Response model for retrieving sensitive words. </summary>
    public class GetSensitiveWordsResponse
    {
        /// <summary> Unique identifier for the sensitive word. </summary>
        public int Id { get; set; }

        /// <summary> The sensitive word. </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary> The date and time when the sensitive word was created. </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary> Parameterless constructor for model binding. </summary>
        public GetSensitiveWordsResponse()
        {
            // Intentionally left blank
        }

        /// <summary>  Initializes a new instance of the GetSensitiveWordsResponse class with the specified identifier, sensitive word, and creation date. </summary>
        /// <param name="id">The unique identifier for the sensitive word response. Must be a positive integer.</param>
        /// <param name="word">The sensitive word represented in the response. Cannot be null or empty.</param>
        /// <param name="createdAt">The date and time when the sensitive word was created.</param>
        public GetSensitiveWordsResponse(int id, string word, DateTime createdAt)
        {
            Id = id;
            Word = word;
            CreatedAt = createdAt;
        }
    }
}