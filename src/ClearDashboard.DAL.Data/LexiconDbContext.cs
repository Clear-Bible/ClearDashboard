using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Xml.Serialization;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class LexiconDbContext : DbContext
    {
        #nullable disable
        private readonly ILogger<LexiconDbContext> _logger;
        public IUserProvider UserProvider { get; set; }
        public string DatabaseName { get; private set; }
        public DbContextOptionsBuilder<LexiconDbContext> OptionsBuilder { get; private set; }

        public LexiconDbContext(ILogger<LexiconDbContext> logger, IUserProvider userProvider, string databaseName, DbContextOptionsBuilder<LexiconDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {
            _logger = logger;
            UserProvider = userProvider;
            DatabaseName = databaseName;
            OptionsBuilder = optionsBuilder;
        }
        internal LexiconDbContext(string databaseName, DbContextOptionsBuilder<LexiconDbContext> optionsBuilder)
            : base(optionsBuilder.Options)
        {
            // This constructor is only for initial migration / design time usage
            DatabaseName = databaseName;
            OptionsBuilder = optionsBuilder;
        }

        public virtual DbSet<LexicalItem> LexicalItems => Set<LexicalItem>();
        public virtual DbSet<LexicalItemDefinition> LexicalItemDefinitions => Set<LexicalItemDefinition>();
        public virtual DbSet<LexicalItemSurfaceText> LexicalItemSurfaceTexts => Set<LexicalItemSurfaceText>();
        public virtual DbSet<SemanticDomain> SemanticDomains => Set<SemanticDomain>();
        public virtual DbSet<SemanticDomainLexicalItemDefinitionAssociation> SemanticDomainLexicalItemDefinitionAssociations => Set<SemanticDomainLexicalItemDefinitionAssociation>();
        public virtual DbSet<User> Users => Set<User>();
        public async Task Migrate()
        {
            try
            {
                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                _logger?.LogInformation("Ensuring that the database is created, migrating if necessary.");

                //todo comment out the if to allow migration for FileNew Projects
                //await Database.GetPendingMigrationsAsync();
                if ((await Database.GetPendingMigrationsAsync()).Any())
                {
                    _logger?.LogInformation("Migration required, migrating...");
                    await Database.MigrateAsync();
                    //await Database.CloseConnectionAsync();

                    await EnsureMigrated();
                }
                _logger?.LogInformation("Check to migrate database complete");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }

        private async Task EnsureMigrated()
        {
            try
            {
                _ = LexicalItems.ToList();
            }
            catch
            {
                _logger.LogInformation($"The migrations for the {DatabaseName} database failed -- forcing the migrations again.");
                await Database.MigrateAsync();
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // We want our table names to be singular
            modelBuilder.RemovePluralizingTableNameConvention();

            // now handle any entities which have DatetimeOffset properties.                                                  
            modelBuilder.AddDateTimeOffsetToBinaryConverter(Database.ProviderName);

            // This ensures that the User Id for the current Dashboard user is always
            // set when an entity is added to the database.
            modelBuilder.AddUserIdValueGenerator();

            modelBuilder
                .Entity<SemanticDomain>()
                .HasMany(p => p.LexicalItemDefinitions)
                .WithMany(p => p.SemanticDomains)
                .UsingEntity<SemanticDomainLexicalItemDefinitionAssociation>();

            modelBuilder.Entity<LexicalItem>()
                .HasIndex(p => new { p.TrainingText, p.Language })
                .IsUnique();
        }
    }
}
