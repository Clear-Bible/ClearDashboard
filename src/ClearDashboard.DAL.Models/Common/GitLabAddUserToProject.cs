using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitLabAddUserToProject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("web_url")]
        public string WebUrl { get; set; }

        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; }

    }
}
