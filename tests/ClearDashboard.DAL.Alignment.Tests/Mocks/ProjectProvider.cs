using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Tests.Mocks
{
    public  class ProjectProvider:  IProjectProvider
    {
        public ProjectInfo? CurrentProject { get; set; }
        public ParatextProject? CurrentParatextProject { get; set; }
        public bool HasCurrentProject => CurrentProject != null;
        public bool HasCurrentParatextProject => CurrentParatextProject != null;
    }
}
