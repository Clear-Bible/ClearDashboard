using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitUser
    {
        public bool IsSelected { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string? UserName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
        
        [JsonPropertyName("organization")]
        public string Organization { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

    }
}
