using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Models.HttpClientFactory
{
    public class GitLabPostUser
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; } = "";

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "";
        [JsonPropertyName("password")]
        public string Password { get; set; } = "";
    }
}
