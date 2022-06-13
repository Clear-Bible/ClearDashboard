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
        public virtual DbSet<NoteAssociation> DataAssociations => Set<NoteAssociation>();
        //public virtual DbSet<InterlinearNote> InterlinearNotes => Set<InterlinearNote>();
        public virtual DbSet<Note> Notes => Set<Note>();
        public virtual DbSet<ParallelCorpus> ParallelCorpa => Set<ParallelCorpus>();
        public virtual DbSet<ParallelVersesLink> ParallelVersesLinks => Set<ParallelVersesLink>();
        public virtual DbSet<ProjectInfo> ProjectInfos => Set<ProjectInfo>();
        public virtual DbSet<QuestionGroup> QuestionGroups => Set<QuestionGroup>();
        public virtual DbSet<RawContent> RawContent => Set<RawContent>();
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
                _logger?.LogInformation("Ensuring that the database is created, migrating if necessary.");

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

            // We want our table names to be singular
            modelBuilder.RemovePluralizingTableNameConvention();

            // NB:  I'm relying on the default naming and relationship conventions from EF Core to set up the database...

            //  **** leaving this here in the event we need to override the default conventions ****
            // NB:  Add the configuration of any newly added 
            //      entities to the ConfigureEntities extension method
            //modelBuilder.ConfigureEntities();
           
            // NB:  Add any new entities which inherit from RawContent
            //      to the ConfigureRawContentEntities extension method
            modelBuilder.ConfigureRawContentEntities();

            // NB:  Add any new entities which inherit from NoteAssociation
            //      to the ConfigureNoteAssociationEntities extension method
            modelBuilder.ConfigureNoteAssociationEntities();

            // now handle any entities which have DatetimeOffset properties.                                                  
            modelBuilder.AddDateTimeOffsetToBinaryConverter(Database.ProviderName);

        }
    }
}
