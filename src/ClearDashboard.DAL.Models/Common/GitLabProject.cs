using System.Security;
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

        [JsonPropertyName("permissions")]
        public Permissions Permissions { get; set; }

        [JsonPropertyName("owner")]
        public Owner RemoteOwner { get; set; }

        public PermissionLevel RemotePermissionLevel
        {
            get
            {
                switch (Permissions.ProjectAccess.AccessLevel)
                {
                    case 30:
                        return PermissionLevel.ReadOnly;
                    case 40:
                        return PermissionLevel.ReadWrite;
                    case 50:
                        return PermissionLevel.Owner;
                }
                return PermissionLevel.Owner;
            }
        }
    }

    public class Permissions
    {
        [JsonPropertyName("project_access")]
        public ProjectAccess ProjectAccess { get; set; }

        [JsonPropertyName("group_access")]
        public object GroupAccess { get; set; }
    }

    public class ProjectAccess
    {
        [JsonPropertyName("access_level")]
        public int AccessLevel { get; set; }

        [JsonPropertyName("notification_level")]
        public int NotificationLevel { get; set; }
    }

   
}
