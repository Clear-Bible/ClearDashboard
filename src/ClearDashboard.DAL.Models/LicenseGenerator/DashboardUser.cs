using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.LicenseGenerator
{
    public class DashboardUser
    {
        public DashboardUser()
        {
        }

        public DashboardUser(User user, string email, string licenseKey, string? organization, int? gitLabUserId, string? appVersionNumber, DateTime appLastDate)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            ParatextUserName = user.ParatextUserName;//many original license didn't have this info
            IsInternal = user.IsInternal ?? false;
            LicenseVersion = user.LicenseVersion ?? 0;

            LicenseKey = licenseKey;
            Email = email;
            Organization = organization;
            GitLabUserId = gitLabUserId ?? -1;
            AppVersionNumber = appVersionNumber;
            AppLastDate = appLastDate;
        }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName => FirstName + " " + LastName;

        [JsonPropertyName("paratextUserName")]
        public string? ParatextUserName { get; set; }

        [JsonPropertyName("organization")]
        public string? Organization { get; set; }

        [JsonPropertyName("isInternal")]
        public bool IsInternal { get; set; }

        [JsonPropertyName("licenseVersion")]
        public int LicenseVersion { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("gitLabUserId")]
        public int GitLabUserId { get; set; }

        [JsonPropertyName("appVersionNumber")]
        public string? AppVersionNumber { get; set; }

        [JsonPropertyName("appLastDate")]
        public DateTime? AppLastDate { get; set; }

        public User ToUser()
        {
            return new User
            {
                Id = Id,
                FirstName = FirstName,
                LastName = LastName,
                ParatextUserName = ParatextUserName,
                IsInternal = IsInternal
            };
        }

    }
}
