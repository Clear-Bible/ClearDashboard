using ClearDashboard.DataAccessLayer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<ProjectAssets> Get(string projectName);
        Task<TDbContext> GetDatabaseContext(string projectName);
    }

    public class ProjectDbContextFactory : IProjectNameDbContextFactory<ProjectDbContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProjectDbContextFactory>? _logger;

        public ProjectDbContextFactory(IServiceProvider serviceProvider, ILogger<ProjectDbContextFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ProjectAssets> Get(string projectName)
        {
            projectName = projectName.Replace(" ", "_");
            var projectAssets = new ProjectAssets
            {
                ProjectName = projectName,
                ProjectDirectory = EnsureProjectDirectory(projectName),
            };

            projectAssets.ProjectDbContext = await GetProjectDbContext(projectAssets.DataContextPath);
            return projectAssets;

        }

        public async Task<ProjectDbContext> GetDatabaseContext(string projectName)
        {
           return await GetProjectDbContext(EnsureProjectDirectory(projectName));
        }

        private async Task<ProjectDbContext> GetProjectDbContext(string fullPath)
        {
            var context = _serviceProvider.GetService<ProjectDbContext>();
            if (context != null)
            {
                try
                {
                    if (_logger != null)
                    {
                        _logger.LogInformation($"Attempting to create or migrate '{fullPath}'");
                    }
                    context.DatabasePath = fullPath;
                    await context.Migrate();
                }
                catch (Exception? ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError(ex, "An error occurred while creating an instance the ProjectDbContext.");
                    }

                    throw;
                }
                return context;
            }
            throw new NullReferenceException("Please ensure 'ProjectDbContext' has been registered with the dependency injection container.");
        }

        private string EnsureProjectDirectory(string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
            {
                throw new ArgumentNullException(nameof(projectName), "A project name must be provided in order for a 'Project' database context to returned.");
            }

            
            var directoryPath = string.Format(FilePathTemplates.ProjectDirectoryTemplate, projectName); 
            if (!Directory.Exists(directoryPath))
            {
                _logger?.LogInformation($"Creating project directory {directoryPath}.");
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }
}
