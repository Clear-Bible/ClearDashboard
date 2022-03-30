using ClearDashboard.DataAccessLayer.Context;

namespace ClearDashboard.DataAccessLayer.Models
{
    public  class ProjectAssets
    {
        public string ProjectName { get; set; }
        public string ProjectDirectory { get; set; }
        public string DataContextPath => $"{ProjectDirectory}\\{ProjectName}.sqlite";
        public AlignmentContext AlignmentContext { get; set; }
    }
}
