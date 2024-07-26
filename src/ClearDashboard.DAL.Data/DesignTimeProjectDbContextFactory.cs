using ClearDashboard.DataAccessLayer.Data.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace ClearDashboard.DataAccessLayer.Data
{
    internal class DesignTimeProjectDbContextFactory : IDesignTimeDbContextFactory<ProjectDbContext>
    {
        public ProjectDbContext CreateDbContext(string[] args)
        {
            // For initial migration
            var optionsBuilder = new DbContextOptionsBuilder<ProjectDbContext>();
            optionsBuilder.UseSqlite("Data Source=initialmigration.db");

            return new ProjectDbContext("initialmigration", optionsBuilder);
        }
    }

    internal class DesignTimeNpgsqlProjectDbContextFactory : IDesignTimeDbContextFactory<NpgsqlProjectDbContext>
    {
        public NpgsqlProjectDbContext CreateDbContext(string[] args)
        {
            var designTimeConnectionString = "Host=host.docker.internal; Database=clearengine; Username=clear; Password=clear";
            var connectionStringsOptions = new OptionsWrapper<ConnectionStringsOptions>(new ConnectionStringsOptions
            {
                ClearDashboardDatabase = designTimeConnectionString
            });

            // For initial migration
            var optionsBuilder = new DbContextOptionsBuilder<NpgsqlProjectDbContext>();
            optionsBuilder
                .UseNpgsql(designTimeConnectionString)
                .ReplaceService<IHistoryRepository, CamelCaseHistoryContext>()
                .UseSnakeCaseNamingConvention();

            return new NpgsqlProjectDbContext(connectionStringsOptions, "initialmigration", optionsBuilder);
        }
    }
#pragma warning disable EF1001 // Internal EF Core API usage.
    public class CamelCaseHistoryContext : NpgsqlHistoryRepository
    {
        public CamelCaseHistoryContext(HistoryRepositoryDependencies dependencies) : base(dependencies)
        {
        }

        protected override void ConfigureTable(EntityTypeBuilder<HistoryRow> history)
        {
            base.ConfigureTable(history);

            history.Property(h => h.MigrationId).HasColumnName("MigrationId");
            history.Property(h => h.ProductVersion).HasColumnName("ProductVersion");
        }
    }
#pragma warning restore EF1001 // Internal EF Core API usage.
}
