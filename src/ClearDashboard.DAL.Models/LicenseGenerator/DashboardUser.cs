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

        public DashboardUser(User user, string email, string licenseKey)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            ParatextUserName = user.ParatextUserName;
            IsInternal = user.IsInternal;
            LicenseVersion = user.LicenseVersion;

            LicenseKey = licenseKey;
            Email = email;
        }

        [JsonPropertyName("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("fullName")]
        public string? FullName => $"{FirstName} {LastName}";

        [JsonPropertyName("paratextUserName")]
        public string? ParatextUserName { get; set; }

        [JsonPropertyName("isInternal")]
        public bool? IsInternal { get; set; } = false;

        [JsonPropertyName("licenseVersion")]
        public int? LicenseVersion { get; set; } = 0;


        [JsonPropertyName("licenseKey")]
        public string? LicenseKey { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

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
