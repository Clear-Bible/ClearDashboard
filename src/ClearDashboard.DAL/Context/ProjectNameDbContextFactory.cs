using System;
using System.IO;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Context
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<ProjectAssets> Create(string connectionString);
    }

    public class ProjectNameDbContextFactory : IProjectNameDbContextFactory<AlignmentContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProjectNameDbContextFactory> _logger;

        public ProjectNameDbContextFactory(IServiceProvider serviceProvider, ILogger<ProjectNameDbContextFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ProjectAssets> Create(string projectName)
        {
            var projectAssets = new ProjectAssets
            {
                ProjectName = projectName,
                ProjectDirectory = EnsureProjectDirectory(projectName),
            };

            projectAssets.AlignmentContext = await GetAlignmentContext(projectAssets.DataContextPath);
            return projectAssets;

        }

        private async Task<AlignmentContext> GetAlignmentContext(string fullPath)
        {
            var context = _serviceProvider.GetService<AlignmentContext>();
            if (context != null)
            {
                try
                {
                    _logger.LogInformation($"Attempting to create or migrate '{fullPath}'");
                    context.DatabasePath = fullPath;
                    await context.Migrate();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while creating an instance the AlignmentContext.");
                    throw;
                }
                return context;
            }
            throw new NullReferenceException("Please ensure 'AlignmentContext' has been registered with the dependency injection container.");
        }

        private string EnsureProjectDirectory(string projectName)
        {
            var directoryPath =  $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogInformation($"Creating project directory {directoryPath}.");
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }
}
