using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectManager
{
    IEnumerable<ProjectInfo> GetAllProjects();
    ProjectInfo LoadProject(string projectName);
    ProjectInfo DeleteProject(string projectName);
    ProjectInfo CreateProject();
}