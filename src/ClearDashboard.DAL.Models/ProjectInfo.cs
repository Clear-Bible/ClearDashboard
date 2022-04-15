namespace ClearDashboard.DataAccessLayer.Models
{
    public partial class ProjectInfo
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public DateTime Created { get; set; }
        public bool IsRtl { get; set; }
        public int? LastContentWordLevel { get; set; }
    }
}
