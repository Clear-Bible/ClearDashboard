using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Settings;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClearDashboard.DataAccessLayer.Data
{
    /// <summary>
    /// Class is only used during ef core migration, if done using something like:
    /// dotnet ef migrations add InitialCreate --context NpgsqlProjectDbContext --output-dir NpgsqlMigrations
    /// </summary>
    public class NpgsqlProjectDbContext : ProjectDbContext
    {
        #nullable disable

        private readonly ConnectionStringsOptions _connectionStringsOptions;

        public NpgsqlProjectDbContext(ILogger<ProjectDbContext> logger, IOptions<ConnectionStringsOptions> options, IMediator mediator, IUserProvider userProvider, string databaseName, DbContextOptionsBuilder<ProjectDbContext> optionsBuilder)
            : base(logger, mediator, userProvider, databaseName, optionsBuilder)
        {
            _connectionStringsOptions = options.Value;
        }

        internal NpgsqlProjectDbContext(IOptions<ConnectionStringsOptions> options, string databaseName, DbContextOptionsBuilder<NpgsqlProjectDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {
            _connectionStringsOptions = options.Value;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options
                .UseNpgsql(_connectionStringsOptions.ClearDashboardDatabase)
                .UseSnakeCaseNamingConvention();
    }
}
