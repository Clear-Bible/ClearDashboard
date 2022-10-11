using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectManager
{
    Task<IEnumerable<Project>> GetAllProjects();
    Task<Project> LoadProject(string projectName);
    Task<Project> DeleteProject(string projectName);
    Task<Project> CreateProject(string projectName);
    Task<Project> UpdateProject(Project project);
}