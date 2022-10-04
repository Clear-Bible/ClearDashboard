using Autofac;
using Autofac.Core;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<ProjectAssets> Get(string projectName, bool migrate = false, ILifetimeScope? serviceScope = null);
        Task<TDbContext> GetDatabaseContext(string projectName, bool migrate = false, ILifetimeScope? serviceScope = null);
    }

    public class ProjectDbContextFactory : IProjectNameDbContextFactory<ProjectDbContext>, IDisposable
    {
        private readonly ILifetimeScope _serviceScope;
        private readonly ILogger<ProjectDbContextFactory>? _logger;

        public ProjectAssets? ProjectAssets { get; private set; }
        public ILifetimeScope ServiceScope => _serviceScope;

        public ProjectDbContextFactory(ILifetimeScope serviceProvider, ILogger<ProjectDbContextFactory> logger)
        {
            _serviceScope = serviceProvider;
            _logger = logger;
        }

        public async Task<ProjectAssets> Get(string projectName, bool migrate = false, ILifetimeScope? serviceScope = null)
        {
            var projectDbContext = await GetDatabaseContext(projectName, migrate, serviceScope);
            ProjectAssets = new ProjectAssets
            {
                ProjectName = projectDbContext.ProjectName,
                ProjectDbContext = projectDbContext // This probably should not be here.  
            };

            return ProjectAssets;
        }

        public async Task<ProjectDbContext> GetDatabaseContext(string projectName, bool migrate = false, ILifetimeScope? contextScope = null)
        {
            var scope = contextScope ?? _serviceScope;

            var context = scope.Resolve<ProjectDbContext>(
                new NamedParameter("projectName", projectName),
                new ResolvedParameter(
                    (pi, cc) => pi.Name == "optionsBuilder",
                    (pi, cc) => cc.Resolve<DbContextOptionsBuilder<ProjectDbContext>>(
                        new NamedParameter("projectName", projectName))));


            if (context != null)
            {
                try
                {
                    //if (_logger != null)
                    //{
                    //    _logger.LogInformation($"Attempting to create or migrate '{fullPath}'");
                    //}

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
