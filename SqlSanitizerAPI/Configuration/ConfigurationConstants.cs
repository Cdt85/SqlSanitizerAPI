namespace SqlSanitizerAPI.Configuration
{
    /// <summary> Application-wide configuration section names and constants. Centralizes identifiers to improve maintainability. </summary>
    public static class ConfigurationSections
    {
        public const string Jwt = "Jwt";
        public const string SanitizationService = "SanitizationService";
        public const string AuthController = "AuthController";
        public const string Database = "DB";
        public const string DefaultConnection = "DefaultConnection";
    }

    /// <summary> Database configuration keys. </summary>
    public static class DatabaseConfigKeys
    {
        public const string DbSchema = "DB:DbSchema";
        public const string SqlCommandDefaultTimeout = "DB:SqlCommandDefaultTimeout";
        public const string LogConnectionMessages = "DB:LogConnectionMessages";
    }
}
