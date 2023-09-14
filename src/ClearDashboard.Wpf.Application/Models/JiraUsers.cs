using System.Text.Json.Serialization;

namespace ClearDashboard.Wpf.Application.Models
{
    public class JiraUser
    {
        [JsonPropertyName("self")]
        public string Self { get; set; } = string.Empty;

        [JsonPropertyName("accountId")]
        public string AccountId { get; set; } = string.Empty;

        [JsonPropertyName("accountType")]
        public string AccountType { get; set; } = string.Empty;

        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; } = string.Empty;

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        public string Password { get; set; } = string.Empty;

    }
}
