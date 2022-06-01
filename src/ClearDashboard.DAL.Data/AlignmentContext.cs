using ClearDashboard.DataAccessLayer.Data.EntityConfiguration;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Logging;
using StringContent = ClearDashboard.DataAccessLayer.Models.StringContent;

namespace ClearDashboard.DataAccessLayer.Data
{
    public class AlignmentContext : DbContext
    {
        private readonly ILogger<AlignmentContext>? _logger;

        public AlignmentContext() : this(string.Empty)
        {
           
        }

        public AlignmentContext(ILogger<AlignmentContext> logger) :  this(string.Empty)
        {
            _logger = logger;
           
        }

        public AlignmentContext(DbContextOptions<AlignmentContext> options)
            : base(options)
        {
            DatabasePath = string.Empty;
        }

        protected AlignmentContext(string databasePath)
        {
            DatabasePath = databasePath;
        }

        public virtual DbSet<Adornment> Adornments => Set<Adornment>();
        public virtual DbSet<Alignment> Alignments => Set<Alignment>();

        public virtual DbSet<AlignmentVersion> AlignmentVersions => Set<AlignmentVersion>();
        public virtual DbSet<Corpus> Corpa => Set<Corpus>();

        public virtual DbSet<InterlinearNote> InterlinearNotes => Set<InterlinearNote>();
        public virtual DbSet<Note> Notes => Set<Note>();
        public virtual DbSet<ParallelCorpus> ParallelCorpa => Set<ParallelCorpus>();
        public virtual DbSet<ParallelVersesLink> ParallelVersesLinks => Set<ParallelVersesLink>();
        public virtual DbSet<ProjectInfo> ProjectInfos => Set<ProjectInfo>();
        public virtual DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
        public virtual DbSet<Token> Tokens => Set<Token>();
        public virtual DbSet<User> Users => Set<User>();
        public virtual DbSet<Verse> Verses => Set<Verse>();
        public virtual DbSet<VerseLink> VerseLinks => Set<VerseLink>();

        public string DatabasePath { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Filename={DatabasePath}");
            }
        }

        public async Task Migrate()
        {
            try
            {
                // Ensure that the database is created.  Note that if we want to be able to apply migrations later,
                // we want to call Database.Migrate(), not Database.EnsureCreated().
                // https://stackoverflow.com/questions/38238043/how-and-where-to-call-database-ensurecreated-and-database-migrate
                _logger?.LogInformation("Ensure that the database is created, migrating if necessary.");

                await Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Could not apply database migrations");

                // This is useful when applying migrations via the EF core plumbing, please leave in place.
                Console.WriteLine($"Could not apply database migrations: {ex.Message}");
            }
        }
      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //This will singularize all table names
            modelBuilder.RemovePluralizingTableNameConvention();

            new AdornmentConfiguration().Configure(modelBuilder.Entity<Adornment>());
            new AlignmentConfiguration().Configure(modelBuilder.Entity<Alignment>());
            new AlignmentVersionConfiguration().Configure(modelBuilder.Entity<AlignmentVersion>());
            new CorpusConfiguration().Configure(modelBuilder.Entity<Corpus>());
            new InterlinearNoteConfiguration().Configure(modelBuilder.Entity<InterlinearNote>());
            new NoteConfiguration().Configure(modelBuilder.Entity<Note>());
            new ParallelCorpusConfiguration().Configure(modelBuilder.Entity<ParallelCorpus>());
            new ParallelVersesLinkConfiguration().Configure(modelBuilder.Entity<ParallelVersesLink>());
            new ProjectInfoConfiguration().Configure(modelBuilder.Entity<ProjectInfo>());
            new QuestionGroupConfiguration().Configure(modelBuilder.Entity<QuestionGroup>());
            new RawContentConfiguration().Configure(modelBuilder.Entity<RawContent>());
            new TokenConfiguration().Configure(modelBuilder.Entity<Token>());
            new UserConfiguration().Configure(modelBuilder.Entity<User>());
            new VerseConfiguration().Configure(modelBuilder.Entity<Verse>());
            new VerseLinkConfiguration().Configure(modelBuilder.Entity<VerseLink>());

            modelBuilder.Entity<StringContent>();
            // now handle any entities which have DatetimeOffset properties.
            modelBuilder.AddDateTimeOffsetToBinaryConverter(Database.ProviderName);

        }
    }
}
