using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class GitLabProject
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("description")]
        public object Description { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("name_with_namespace")]
        public string NameWithNamespace { get; set; }
        
        [JsonPropertyName("path")]
        public string Path { get; set; }
        
        [JsonPropertyName("path_with_namespace")]
        public string PathWithNamespace { get; set; }
        
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonPropertyName("default_branch")]
        public string DefaultBranch { get; set; }
        
        [JsonPropertyName("tag_list")]
        public List<object> TagList { get; set; }
        
        [JsonPropertyName("topics")]
        public List<object> Topics { get; set; }

        [JsonPropertyName("http_url_to_repo")]
        public string HttpUrlToRepo { get; set; }
        
        [JsonPropertyName("web_url")]
        public string WebUrl { get; set; }

    }
}
