using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models
{
    public class GitAccessToken
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("revoked")]
        public bool Revoked { get; set; }
        
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = "";

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; } = new();

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("last_used_at")]
        public string LastUsedAt { get; set; } = "";

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("expires_at")]
        public string ExpiresAt { get; set; } = "";

        [JsonPropertyName("token")]
        public string Token { get; set; } = "";

    }
}
