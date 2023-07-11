
namespace ClearDashboard.DataAccessLayer.Models
{
    public class DashboardCollabProject 
    {
        public DashboardCollabProject()
        {
        }

        public bool IsCompatibleVersion { get; set; } = true;

        public bool IsInitialized { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string? AppVersion { get; set; }
        public DateTimeOffset? Created { get; set; }
        public Guid ProjectId { get; set; } = Guid.Empty;

        public string RemoteOwner { get; set; } = string.Empty;
        public PermissionLevel? PermissionLevel { get; set; } = Models.PermissionLevel.Owner;
    }
}
