namespace SqlSanitizerAPI.Services.SanitizationService
{
    /// <summary>  Provides strongly-typed cache key generation to avoid magic strings and potential errors. </summary>
    public static class CacheKeys
    {
        /// <summary> Prefix for all sensitive word related cache keys. </summary>
        private const string SensitiveWordsPrefix = "SensitiveWords";

        /// <summary> Gets the cache key for all sensitive words. </summary>
        public static string AllSensitiveWords => $"{SensitiveWordsPrefix}_All";

        /// <summary> Gets the cache key for a specific sensitive word by ID. </summary>
        /// <param name="id">The word ID.</param>
        public static string SensitiveWordById(int id) => $"{SensitiveWordsPrefix}_{id}";

        /// <summary> Gets all possible cache keys for sensitive words</summary>
        /// <returns>Array of cache key patterns.</returns>
        public static string[] GetAllSensitiveWordPatterns() => new[]
        {
            AllSensitiveWords,
            $"{SensitiveWordsPrefix}_*" // Pattern for all ID-based keys
        };
    }
}
