using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Tests.Mocks
{
    public  class ProjectProvider:  IProjectProvider
    {
        public ProjectInfo? CurrentProject { get; set; }
        public Project? CurrentParatextProject { get; set; }
    }
}
