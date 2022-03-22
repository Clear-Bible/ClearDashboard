using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Context
{
    public interface IProjectNameDbContextFactory<TDbContext> where TDbContext : DbContext
    {
        Task<TDbContext> Create(string connectionString);
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

        public async Task<AlignmentContext> Create(string projectName)
        {
            var directoryPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Personal)}\\ClearDashboard_Projects\\{projectName}";
            if (!Directory.Exists(directoryPath))
            {
                _logger.LogInformation($"Creating project directory {directoryPath}.");
                Directory.CreateDirectory(directoryPath);
            }
            var fullPath = $"{directoryPath}\\{projectName}.sqlite";

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
    }
}
