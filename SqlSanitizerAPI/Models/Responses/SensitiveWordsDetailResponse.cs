namespace SqlSanitizerAPI.Models.Responses
{
    /// <summary> Represents the response containing details of a sensitive word, including its unique identifier and the word itself. </summary>
    /// <remarks>This class is used to encapsulate the details of a sensitive word entry, providing a structured response for API consumers.</remarks>
    public class SensitiveWordsDetailResponse
    {

        /// <summary> Unique identifier for the sensitive word. </summary>
        public int Id { get; set; }

        /// <summary> The sensitive word. </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary> Parameterless constructor for model binding. </summary>
        public SensitiveWordsDetailResponse() 
        {
            // Intentionally left blank
        }

        /// <summary> Initializes a new instance of the SensitiveWordsDetailResponse class with the specified identifier and sensitive word. </summary>
        /// <param name="id">The unique identifier for the sensitive word entry. Must be a non-negative integer.</param>
        /// <param name="word">The sensitive word to be associated with this response. Cannot be null or empty.</param>
        public SensitiveWordsDetailResponse(int id, string word)
        {
            Id = id;
            Word = word;
        }
    }
}
