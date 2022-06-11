namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ProjectInfo : ClearEntity
    {
        public string? ProjectName { get; set; }
        public bool IsRtl { get; set; }
        public int? LastContentWordLevel { get; set; }
    }
}
