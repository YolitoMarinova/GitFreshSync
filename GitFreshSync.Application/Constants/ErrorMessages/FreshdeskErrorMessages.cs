namespace GitFreshSync.Application.Constants.ErrorMessages
{
    public static class FreshdeskErrorMessages
    {
        public const string SearchCompaniesFailed = "Failed to search companies. Status code: {0}";
        public const string DeserializationFailed = "Failed to deserialize company/ies.";
        public const string FailedToCreateCompany = "Failed to create company. Status code: {0}";
        public const string SearchContanctsFailed = "Failed to search contacts. Status code: {0}";
        public const string FailedToCreateContact = "Failed to create contact. Status code: {0}";
        public const string FailedToUpdateContact = "Failed to update contact. Status code: {0}";
    }
}
