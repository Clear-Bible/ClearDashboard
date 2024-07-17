using System.Text.Json.Serialization;

namespace ClearDashboard.DataAccessLayer.Models.Common
{
    public class CollaborationUser
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("remoteUserName")]
        public string RemoteUserName { get; set; }

        [JsonPropertyName("remoteEmail")]
        public string RemoteEmail { get; set; }

        [JsonPropertyName("remotePersonalAccessToken")]
        public string RemotePersonalAccessToken { get; set; }

        [JsonPropertyName("remotePersonalPassword")]
        public string RemotePersonalPassword { get; set; }

        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        [JsonPropertyName("namespaceId")]
        public int NamespaceId { get; set; }

        [JsonPropertyName("tokenId")]
        public int TokenId { get; set; }
    }
}
