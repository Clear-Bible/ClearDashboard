using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitLabAddUserToProject
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("state")]
        public string state { get; set; }

        [JsonPropertyName("web_url")]
        public string web_url { get; set; }

        [JsonPropertyName("access_level")]
        public int access_level { get; set; }

    }
}
