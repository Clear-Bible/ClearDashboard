using ClearDashboard.DataAccessLayer.Data.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class NpgSqlProjectDbContextOptionsBuilder<T> : DbContextOptionsBuilder<T> where T : DbContext
    {
        private readonly ILogger<SqliteProjectDbContextOptionsBuilder<T>> _logger;
        private readonly ConnectionStringsOptions _options;

        public string DatabaseName { get; init; } = string.Empty;

        public override DbContextOptions<T> Options
        {
            get 
            {
                return NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql<T>(
                    new(),
                    _options.ClearDashboardDatabase)
                        .ReplaceService<IHistoryRepository, CamelCaseHistoryContext>()
                        .UseSnakeCaseNamingConvention().Options;
            }
        }

        public NpgSqlProjectDbContextOptionsBuilder(ILogger<SqliteProjectDbContextOptionsBuilder<T>> logger, IOptions<ConnectionStringsOptions> options, string databaseName)
        {
            _logger = logger;
            _options = options.Value;
            DatabaseName = databaseName;
        }
    }
}
