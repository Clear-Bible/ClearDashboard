namespace ClearDashboard.DataAccessLayer.Data.Models
{
    public  class ProjectAssets
    {
        public string? ProjectName { get; set; }
        public string? ProjectDirectory { get; set; }
        public string DataContextPath => $"{ProjectDirectory}\\{ProjectName}.sqlite";
        public ProjectDbContext ProjectDbContext { get; set; }
    }
}
