using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class SqliteProjectDbContextOptionsBuilder : DbContextOptionsBuilder<ProjectDbContext>
    {
        private readonly ILogger<SqliteProjectDbContextOptionsBuilder> _logger;
        public string DatabaseName { get; private set; }
        public string DatabaseDirectory { get; private set; }

        public override DbContextOptions<ProjectDbContext> Options
        {
            get 
            {
                return SqliteDbContextOptionsBuilderExtensions.UseSqlite<ProjectDbContext>(
                    new(),
                    $"DataSource={DatabaseDirectory}\\{DatabaseName}.sqlite;Pooling=true;Mode=ReadWriteCreate",
                    options => options.CommandTimeout(600)).Options;
            }
        }

        public SqliteProjectDbContextOptionsBuilder(ILogger<SqliteProjectDbContextOptionsBuilder> logger, string databaseName)
        {
            _logger = logger;
            DatabaseName = databaseName;
            DatabaseDirectory = EnsureDatabaseDirectory(DatabaseName);
        }

        private string EnsureDatabaseDirectory(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName), "A project name must be provided in order for a 'Project' database context to returned.");
            }

            // Use the 'dashboard directory path' as the database folder:
            var directoryPath = string.Format(FilePathTemplates.ProjectDirectoryTemplate, databaseName);
            if (!Directory.Exists(directoryPath))
            {
                _logger?.LogInformation($"Creating database directory {directoryPath}.");
                Directory.CreateDirectory(directoryPath);
            }

            return directoryPath;
        }
    }
}
