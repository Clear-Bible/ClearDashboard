using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class SqliteProjectDbContextOptionsBuilder : DbContextOptionsBuilder<ProjectDbContext>
    {
        private readonly ILogger<SqliteProjectDbContextOptionsBuilder> _logger;
        public string ProjectName { get; private set; }
        public string ProjectDirectory { get; private set; }

        public override DbContextOptions<ProjectDbContext> Options
        {
            get 
            {
                return SqliteDbContextOptionsBuilderExtensions.UseSqlite<ProjectDbContext>(
                    new(),
                    $"DataSource={ProjectDirectory}\\{ProjectName}.sqlite;Pooling=true;Mode=ReadWriteCreate").Options;
            }
        }

        public SqliteProjectDbContextOptionsBuilder(ILogger<SqliteProjectDbContextOptionsBuilder> logger, string projectName)
        {
            _logger = logger;
            ProjectName = projectName.Replace(" ", "_");
            ProjectDirectory = EnsureProjectDirectory(ProjectName);
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
