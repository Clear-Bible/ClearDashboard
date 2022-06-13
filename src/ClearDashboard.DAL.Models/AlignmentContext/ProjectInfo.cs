namespace ClearDashboard.DataAccessLayer.Models
{
    public class ProjectInfo : TimestampedEntity
    {
        public string? ProjectName { get; set; }
        public bool IsRtl { get; set; }
        public int? LastContentWordLevel { get; set; }
    }
}
