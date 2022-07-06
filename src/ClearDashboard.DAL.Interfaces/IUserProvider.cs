using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces
{
    public interface IUserProvider
    {
        User CurrentUser { get; set; }
    }

    public interface IProjectProvider
    {
        ProjectInfo? CurrentProject { get; set; }
        Project? CurrentParatextProject { get; set; }
    }

    public interface IProjectManager
    {
        IEnumerable<ProjectInfo> GetAllProjects();
        ProjectInfo LoadProject(string projectName);
        ProjectInfo DeleteProject(string projectName);
        ProjectInfo CreateProject();
    }
}
