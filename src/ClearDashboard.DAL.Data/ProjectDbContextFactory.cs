using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Interceptors;
using ClearDashboard.DataAccessLayer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<ProjectAssets> Get(string projectName, bool migrate = false);
        Task<TDbContext> GetDatabaseContext(string projectName, bool migrate = false);
    }

    public class ProjectDbContextFactory : IProjectNameDbContextFactory<ProjectDbContext>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProjectDbContextFactory>? _logger;

        public ProjectAssets? ProjectAssets { get; private set; }

        public ProjectDbContextFactory(IServiceProvider serviceProvider, ILogger<ProjectDbContextFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ProjectAssets> Get(string projectName, bool migrate = false)
        {
            projectName = projectName.Replace(" ", "_");
            ProjectAssets = new ProjectAssets
            {
                ProjectName = projectName,
                ProjectDirectory = EnsureProjectDirectory(projectName),
            };

            ProjectAssets.ProjectDbContext = await GetProjectDbContext(ProjectAssets.DataContextPath, migrate);
            return ProjectAssets;
        }

        public async Task<ProjectDbContext> GetDatabaseContext(string projectName, bool migrate = false)
        {
           return await GetProjectDbContext(EnsureProjectDirectory(projectName), migrate);
        }

        private async Task<ProjectDbContext> GetProjectDbContext(string fullPath, bool migrate = false)
        {
            var context = _serviceProvider.GetService<ProjectDbContext>();

            //var userProvider = _serviceProvider.GetService<IUserProvider>();
            //var sqliteDatabaseInterceptorLogger =
            //    _serviceProvider.GetService<ILogger<SqliteDatabaseConnectionInterceptor>>();
            //var connectionInterceptor = new SqliteDatabaseConnectionInterceptor(sqliteDatabaseInterceptorLogger, this);

            //var dcContextLogger = _serviceProvider.GetService<ILogger<ProjectDbContext>>();
            //var context = new ProjectDbContext(dcContextLogger, userProvider, connectionInterceptor);

            if (context != null)
            {
                try
                {
                    //if (_logger != null)
                    //{
                    //    _logger.LogInformation($"Attempting to create or migrate '{fullPath}'");
                    //}
                    context.DatabasePath = fullPath;

                    if (migrate)
                    {
                        await context.Migrate();
                    }
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

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
            ProjectAssets?.ProjectDbContext?.Dispose();

        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ProjectDbContextFactory()
        {
            ReleaseUnmanagedResources();
        }
    }
}
