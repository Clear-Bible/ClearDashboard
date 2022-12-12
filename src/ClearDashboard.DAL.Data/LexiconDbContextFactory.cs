using Autofac;
using Autofac.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class LexiconDbContextFactory
    {
        private readonly ILifetimeScope _serviceScope;
        private readonly ILogger<LexiconDbContextFactory>? _logger;

        public ILifetimeScope ServiceScope => _serviceScope;

        public LexiconDbContextFactory(ILifetimeScope serviceProvider, ILogger<LexiconDbContextFactory> logger)
        {
            _serviceScope = serviceProvider;
            _logger = logger;
        }

        public async Task<LexiconDbContext> GetDatabaseContext(bool migrate = false, ILifetimeScope? contextScope = null)
        {
            var scope = contextScope ?? _serviceScope;
            var databaseName = "Lexicon";

            var context = scope.Resolve<LexiconDbContext>(
                new NamedParameter("databaseName", databaseName),
                new ResolvedParameter(
                    (pi, cc) => pi.Name == "optionsBuilder",
                    (pi, cc) => cc.Resolve<DbContextOptionsBuilder<LexiconDbContext>>(
                        new NamedParameter("databaseName", databaseName))));

            if (context != null)
            {
                try
                {
                    if (migrate)
                    {
                        await context.Migrate();
                    }
                }
                catch (Exception? ex)
                {
                    _logger?.LogError(ex, "An error occurred while creating an instance the ProjectDbContext.");
                    throw;
                }
                return context;
            }
            throw new NullReferenceException("Please ensure 'LexiconDbContext' has been registered with the dependency injection container.");
        }
    }
}
