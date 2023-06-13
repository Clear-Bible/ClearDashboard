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
        public string Description { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("http_url_to_repo")]
        public string HttpUrlToRepo { get; set; }

        [JsonPropertyName("namespace")]
        public Namespace Namespace { get; set; }

        [JsonPropertyName("owner")]
        public Owner Owner { get; set; }
    }

    public class Owner
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("web_url")]
        public string WebUrl { get; set; }
    }

    public class Namespace
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("full_path")]
        public string FullPath { get; set; }

        [JsonPropertyName("parent_id")]
        public object ParentId { get; set; }

        [JsonPropertyName("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonPropertyName("web_url")]
        public string WebUrl { get; set; }
    }
}
