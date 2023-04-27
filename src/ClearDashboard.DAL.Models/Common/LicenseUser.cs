namespace ClearDashboard.DataAccessLayer.Models
{
    //public class LicenseUser : IdentifiableEntity
    //{
    //    public string? FirstName { get; set; }
    //    public string? LastName { get; set; }
    //    public string? LicenseKey { get; set; }
    //    public string FullName => $"{FirstName}, {LastName}";
    //    public string ParatextUserName { get; set; } = null;
    //    public LicenseUserMatchType MatchType { get; set; }
    //}

    public class TemporaryLicenseUser 
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? LicenseKey { get; set; }
        public string FullName => $"{FirstName}, {LastName}";
        public string ParatextUserName { get; set; } = null;
        public LicenseUserMatchType MatchType { get; set; }
        public string Id { get; set; }
    }

    public enum LicenseUserMatchType
    {
        Match,
        FirstNameMismatch,
        LastNameMismatch,
        BothNameMismatch,
        Error
    }
}
