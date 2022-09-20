using ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Interfaces;

public interface IProjectManager
{
    Task<IEnumerable<Project>> GetAllProjects();
    Task<IEnumerable<Project>> LoadProject(string projectName);
    Task<IEnumerable<Corpus>> LoadCorpora(string projectName);
    Task<Project> DeleteProject(string projectName);
    Task<Project> CreateProject(string projectName);
    Task UpdateProject(Project project);
}