using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitLabProjectOwner
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("http_url_to_repo")]
        public string http_url_to_repo { get; set; }

        [JsonPropertyName("namespace")]
        public Namespace @namespace { get; set; }

        [JsonPropertyName("owner")]
        public Owner owner { get; set; }
    }

    public class Owner
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("username")]
        public string username { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("state")]
        public string state { get; set; }

        [JsonPropertyName("avatar_url")]
        public string avatar_url { get; set; }

        [JsonPropertyName("web_url")]
        public string web_url { get; set; }
    }

    public class Namespace
    {
        [JsonPropertyName("id")]
        public int id { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("path")]
        public string path { get; set; }

        [JsonPropertyName("kind")]
        public string kind { get; set; }

        [JsonPropertyName("full_path")]
        public string full_path { get; set; }

        [JsonPropertyName("parent_id")]
        public object parent_id { get; set; }

        [JsonPropertyName("avatar_url")]
        public string avatar_url { get; set; }

        [JsonPropertyName("web_url")]
        public string web_url { get; set; }
    }
}
