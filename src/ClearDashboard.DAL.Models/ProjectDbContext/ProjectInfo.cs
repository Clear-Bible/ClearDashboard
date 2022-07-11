namespace ClearDashboard.DataAccessLayer.Models
{
    public class ProjectInfo : SynchronizableTimestampedEntity
    {
        public string? ProjectName { get; set; }
        public bool IsRtl { get; set; }
        public int? LastContentWordLevel { get; set; }
    }
}
