using System.Text.Json.Serialization;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Services;
public class GitCollaboration
{
    [JsonPropertyName("Collaboration")]
    public Models.CollaborationConfiguration GitAccessToken { get; set; }
}