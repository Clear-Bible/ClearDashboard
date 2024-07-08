using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models.HttpClientFactory
{
    public class GitLabUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string UserName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";

        [JsonPropertyName("organization")]
        public string Organization { get; set; } = "";

        [JsonPropertyName("namespace_id")]
        public int NamespaceId { get; set; }

        [JsonPropertyName("tokenId")]
        public int TokenId { get; set; }

        public string Password { get; set; } = "";
    }
}
