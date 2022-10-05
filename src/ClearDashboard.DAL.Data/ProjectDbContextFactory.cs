using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<TDbContext> GetDatabaseContext(string projectName, bool migrate = false, ILifetimeScope? serviceScope = null);
    }

    public class ProjectDbContextFactory : IProjectNameDbContextFactory<ProjectDbContext>
    {
        private readonly ILifetimeScope _serviceScope;
        private readonly ILogger<ProjectDbContextFactory>? _logger;

        public ILifetimeScope ServiceScope => _serviceScope;

        public ProjectDbContextFactory(ILifetimeScope serviceProvider, ILogger<ProjectDbContextFactory> logger)
        {
            _serviceScope = serviceProvider;
            _logger = logger;
        }

        public async Task<ProjectDbContext> GetDatabaseContext(string projectName, bool migrate = false, ILifetimeScope? contextScope = null)
        {
            var scope = contextScope ?? _serviceScope;
            var databaseName = ConvertProjectNameToSanitizedName(projectName);

            var context = scope.Resolve<ProjectDbContext>(
                new NamedParameter("databaseName", databaseName),
                new ResolvedParameter(
                    (pi, cc) => pi.Name == "optionsBuilder",
                    (pi, cc) => cc.Resolve<DbContextOptionsBuilder<ProjectDbContext>>(
                        new NamedParameter("databaseName", databaseName))));

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
        public static string ConvertProjectNameToSanitizedName(string projectName)
        {
            return projectName.Replace(" ", "_");
        }
    }
}
