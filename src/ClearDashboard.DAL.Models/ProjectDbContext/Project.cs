namespace ClearDashboard.DataAccessLayer.Models
{
    public class Project : SynchronizableTimestampedEntity
    {
        public string? ProjectName { 
            get; 
            set; }
        public bool IsRtl { get; set; }
        public string? DesignSurfaceLayout { get; set; }
        public string? WindowTabLayout { get; set; }
        public string? AppVersion { get; set; }
        public string? LastMergedCommitSha { get; set; }
    }
}
