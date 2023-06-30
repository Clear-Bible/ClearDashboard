using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.LicenseGenerator
{
    public class DashboardUser:User
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
