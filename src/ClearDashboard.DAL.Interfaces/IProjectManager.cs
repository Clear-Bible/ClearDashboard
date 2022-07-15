using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectManager
{
    IEnumerable<Project> GetAllProjects();
    Project LoadProject(string projectName);
    Project DeleteProject(string projectName);
    Project CreateProject();
}